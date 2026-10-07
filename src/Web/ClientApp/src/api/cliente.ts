/** Erro devolvido pela API (ProblemDetails do ASP.NET Core), já com uma mensagem para mostrar. */
export class ErroApi extends Error {
  readonly estado: number;

  constructor(estado: number, mensagem: string) {
    super(mensagem);
    this.estado = estado;
  }
}

interface ProblemDetails {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]> | string[];
}

const mensagensPorEstado: Record<number, string> = {
  401: 'A sessão terminou. Volte a entrar.',
  403: 'Não tem permissão para fazer isto.',
  404: 'Não encontrado.',
};

async function lerErro(resposta: Response): Promise<ErroApi> {
  let mensagem = mensagensPorEstado[resposta.status] ?? `Erro ${resposta.status}.`;
  try {
    const texto = await resposta.text();
    if (texto) {
      const corpo = JSON.parse(texto) as ProblemDetails | string;
      if (typeof corpo === 'string') {
        mensagem = corpo;
      } else if (corpo.errors) {
        const erros = Array.isArray(corpo.errors) ? corpo.errors : Object.values(corpo.errors).flat();
        if (erros.length > 0) mensagem = erros.join(' ');
      } else if (corpo.detail) {
        mensagem = corpo.detail;
      } else if (corpo.title && resposta.status !== 401) {
        mensagem = corpo.title;
      }
    }
  } catch {
    // Corpo sem JSON: fica a mensagem por omissão.
  }
  return new ErroApi(resposta.status, mensagem);
}

/** Pedido à API com o cookie de sessão. Devolve o JSON da resposta (ou undefined quando não há corpo). */
export async function pedido<T>(metodo: string, url: string, corpo?: unknown): Promise<T> {
  const ehFormulario = corpo instanceof FormData;
  const resposta = await fetch(url, {
    method: metodo,
    credentials: 'include',
    headers: corpo === undefined || ehFormulario ? undefined : { 'Content-Type': 'application/json' },
    body: corpo === undefined ? undefined : ehFormulario ? corpo : JSON.stringify(corpo),
  });

  if (!resposta.ok) {
    throw await lerErro(resposta);
  }

  const texto = await resposta.text();
  return (texto ? JSON.parse(texto) : undefined) as T;
}

export const obter = <T>(url: string) => pedido<T>('GET', url);
export const enviar = <T = void>(url: string, corpo?: unknown) => pedido<T>('POST', url, corpo ?? {});
