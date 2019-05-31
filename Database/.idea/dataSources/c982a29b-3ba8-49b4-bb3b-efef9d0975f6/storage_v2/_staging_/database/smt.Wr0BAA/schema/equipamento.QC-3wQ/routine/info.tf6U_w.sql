create or replace function info(info json)
  returns SETOF json
language plpgsql
as $$
BEGIN
  RETURN QUERY
  SELECT array_to_json(array_agg(row_to_json(equipamento_info))) AS controladora
  FROM (
         SELECT
           filedate,
           condominio,
           num,
           descricao,
           tipo_conexao,
           endereco,
           porta,
           habilitado,
           tempo_reconexao,
           fabricante,
           (SELECT array_to_json(array_agg(row_to_json(leitores)))
            FROM (
                   SELECT
                     leitor.filedate,
                     leitor.num,
                     leitor.controladora,
                     leitor.descricao,
                     leitor.porta,
                     leitor.procedimento,
                     leitor.idade_minima,
                     array_to_json(array_agg(row_to_json(acionador.*))) AS acionadores
                   FROM
                     equipamento.leitor
                     LEFT JOIN equipamento.acionador
                       ON leitor.controladora = acionador.controladora AND leitor.num = acionador.leitor
                   --WHERE leitor.controladora = (info ->> 'controladora') :: INTEGER
                   GROUP BY leitor.filedate,
                     leitor.filedate, leitor.num, leitor.controladora, leitor.descricao, leitor.porta,
                     leitor.procedimento
                 ) AS leitores
           ) AS leitores
         FROM equipamento.controladora
         --WHERE num = (info ->> 'controladora') :: INTEGER
       ) AS equipamento_info;
END;
$$;

