/**
 * O NOME DA LOJA EM DUAS PARTES — a marca e a cidade ("Tracbel Agro — Franca"), como as filiais se chamam em produção e
 * como as maquetes de 02/10/2026 escrevem.
 *
 * É O QUE DEIXA A TELA ESTREITA ESCONDER SÓ A MARCA: com a coluna curta, "Tracbel Agro — B…" em todas as linhas não diz
 * qual loja é; a cidade sozinha diz. Quem esconde é o CSS da tela (`.nome-da-loja-marca`), e o nome inteiro fica na dica
 * da célula. Nome fora do padrão sai inteiro, sem partir.
 */

const PADRAO = /^(Tracbel Agro\s*[—–-]\s*)(.+)$/;

export function NomeDaLoja({ nome }: { nome: string }) {
  const partes = PADRAO.exec(nome);
  if (!partes) return <>{nome}</>;
  return (
    <>
      <span className="nome-da-loja-marca">{partes[1]}</span>
      {partes[2]}
    </>
  );
}
