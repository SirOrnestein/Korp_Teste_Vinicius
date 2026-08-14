import { Component, OnInit, signal } from '@angular/core';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [HttpClientModule, FormsModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  produtos = signal<any[]>([]);
  notas = signal<any[]>([]);

  codigo = '';
  descricao = '';
  saldo = 0;
  mensagemProduto = signal('');
  tipoMensagemProduto = signal('');

  mensagemNota = signal('');
  tipoMensagemNota = signal('');

  mensagemImpressao = signal('');
  tipoMensagemImpressao = signal('');

  processandoNotaId = signal<number | null>(null);

  cliente = '';
  codigoProdutoNota = '';
  quantidadeNota = 1;
  itensNota: any[] = [];

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.carregarProdutos();
    this.carregarNotas();
  }

  adicionarItem(): void {
  if (!this.codigoProdutoNota || this.quantidadeNota <= 0) {
    return;
  }

  const produto = this.produtos().find(
    p => p.codigo === this.codigoProdutoNota
  );

  if (!produto) {
    return;
  }

  this.itensNota.push({
    codigoProduto: this.codigoProdutoNota,
    descricao: produto.descricao,
    quantidade: this.quantidadeNota
  });

  this.codigoProdutoNota = '';
  this.quantidadeNota = 1;
}

  carregarProdutos(): void {
    this.http.get<any[]>('http://localhost:5255/produtos')
      .subscribe({
        next: (dados) => this.produtos.set(dados),
        error: (erro) => console.error('Erro ao carregar produtos:', erro)
      });
  }

  carregarNotas(): void {
    this.http.get<any[]>('http://localhost:5026/notas')
      .subscribe({
        next: (dados) => this.notas.set(dados),
        error: (erro) => console.error('Erro ao carregar notas:', erro)
      });
  }

  cadastrarProduto(): void {
  const novoProduto = {
    codigo: this.codigo,
    descricao: this.descricao,
    saldo: this.saldo
  };

  this.http.post('http://localhost:5255/produtos', novoProduto)
    .subscribe({
      next: () => {
        this.mensagemProduto.set('Produto cadastrado com sucesso!');
        this.tipoMensagemProduto.set('sucesso');
        setTimeout(() => {
        this.mensagemProduto.set('');
}, 3000);


        this.codigo = '';
        this.descricao = '';
        this.saldo = 0;

        this.carregarProdutos();
      },
      error: () => {
        this.mensagemProduto.set('Não foi possível cadastrar o produto.');
        this.tipoMensagemProduto.set('erro');
        setTimeout(() => {
        this.mensagemProduto.set('');
}, 3000);
      }
    });
}

  criarNota(): void {
    if (!this.cliente.trim()) {
  this.mensagemNota.set('Informe o cliente.');
  this.tipoMensagemNota.set('erro');
  return;
}

if (this.itensNota.length === 0) {
  this.mensagemNota.set('Adicione pelo menos um produto à nota.');
  this.tipoMensagemNota.set('erro');
  return;
}
    const novaNota = {
      cliente: this.cliente,
      itens: this.itensNota.map(item => ({
  codigoProduto: item.codigoProduto,
  quantidade: item.quantidade
}))
    };

    this.http.post('http://localhost:5026/notas', novaNota)
      .subscribe({
        next: () => {
          this.mensagemNota.set('Nota fiscal criada com sucesso!');
          this.tipoMensagemNota.set('sucesso');
          setTimeout(() => {
  this.mensagemNota.set('');
}, 3000);
          this.cliente = '';
          this.codigoProdutoNota = '';
          this.quantidadeNota = 1;
          this.itensNota = [];
          this.carregarNotas();
        },
        error: () => {
          this.mensagemNota.set('Não foi possível criar a nota fiscal.');
          this.tipoMensagemNota.set('erro');
        }
      });
  }

 imprimirNota(id: number): void {
  const confirmar = confirm(
    'Tem certeza que deseja imprimir e fechar esta nota?\n\nO estoque será baixado.'
  );

  if (!confirmar) {
    return;
  }

  this.processandoNotaId.set(id);

  const inicio = Date.now();

  this.http.post(`http://localhost:5026/notas/${id}/imprimir`, {})
    .subscribe({
      next: () => {
        const tempoDecorrido = Date.now() - inicio;
        const tempoRestante = Math.max(700 - tempoDecorrido, 0);

        setTimeout(() => {
          this.processandoNotaId.set(null);

          this.mensagemImpressao.set(
            'Nota fiscal fechada e estoque atualizado com sucesso!'
          );
          this.tipoMensagemImpressao.set('sucesso');

          this.carregarNotas();
          this.carregarProdutos();
        }, tempoRestante);
      },

      error: () => {
        const tempoDecorrido = Date.now() - inicio;
        const tempoRestante = Math.max(700 - tempoDecorrido, 0);

        setTimeout(() => {
          this.processandoNotaId.set(null);

          this.mensagemImpressao.set(
            'Não foi possível fechar a nota fiscal.'
          );
          this.tipoMensagemImpressao.set('erro');
        }, tempoRestante);
      }
    });
}
}