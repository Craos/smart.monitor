-- noinspection SpellCheckingInspectionForFile

--
-- PostgreSQL database dump
--

-- Dumped from database version 10.6
-- Dumped by pg_dump version 10.6

-- Started on 2019-02-05 10:40:20

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

DROP DATABASE smart;
--
-- TOC entry 2272 (class 1262 OID 16384)
-- Name: smart; Type: DATABASE; Schema: -; Owner: -
--

CREATE DATABASE smart WITH TEMPLATE = template0 ENCODING = 'UTF8' LC_COLLATE = 'Portuguese_Brazil.1252' LC_CTYPE = 'Portuguese_Brazil.1252';


\connect smart

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 1 (class 3079 OID 12278)
-- Name: plpgsql; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS plpgsql WITH SCHEMA pg_catalog;


--
-- TOC entry 2274 (class 0 OID 0)
-- Dependencies: 1
-- Name: EXTENSION plpgsql; Type: COMMENT; Schema: -; Owner: -
--

COMMENT ON EXTENSION plpgsql IS 'PL/pgSQL procedural language';


--
-- TOC entry 2 (class 3079 OID 20959)
-- Name: dblink; Type: EXTENSION; Schema: -; Owner: -
--

CREATE EXTENSION IF NOT EXISTS dblink WITH SCHEMA public;


--
-- TOC entry 2275 (class 0 OID 0)
-- Dependencies: 2
-- Name: EXTENSION dblink; Type: COMMENT; Schema: -; Owner: -
--

COMMENT ON EXTENSION dblink IS 'connect to other PostgreSQL databases from within a database';


--
-- TOC entry 262 (class 1255 OID 31048)
-- Name: atualizarcadastros(character varying, bigint); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.atualizarcadastros(origem character varying, limite bigint) RETURNS integer
    LANGUAGE plpgsql
    AS $_X$
DECLARE
  totalregistros int;
  downloaditem   record;
  item           record;
  linhalimite int;
BEGIN

  totalregistros := 0;
  linhalimite := 0;

  -- Inicia a conexao com o banco de dados central
  PERFORM dblink_connect(origem);

  /* Inicio do procedimento de download */

  -- Remove as chaves de comparacao
  TRUNCATE TABLE chaves_veiculos;

  -- Cria uma tabela temporária para armazenar as matriculas
  INSERT INTO chaves_veiculos
  SELECT num
    FROM dblink($_$
         SELECT num
           FROM condominio.veiculos
          WHERE num notnull and autenticacao notnull
    $_$) AS t(num int);

  -- Compara e obtem as matriculas que ainda não existem na tabela
  FOR downloaditem IN
      SELECT chaves_veiculos.num
        FROM chaves_veiculos
        LEFT JOIN veiculos ON chaves_veiculos.num = veiculos.num
       WHERE veiculos.num ISNULL
  LOOP

    SELECT *
      INTO item
      FROM dblink($_$
      insert into veiculos (filedate, timerg, uidins, num, condominio, bloco, andar, unidade, autenticacao, vaga, vaga_locada, modelo, cor, descricao, foto, observacao, marca, placa_letras, placa_numeros, autenticacao_visual, bloqueio, portas, controle_porteiro, tipo_veiculo, situacao_estacionamento, lastdate, lasttime, lastuser, p1_data, p1_hora, p2_data, p2_hora, p4_data, p4_hora, proprietario, serial, new_num)
           select filedate, timerg, uidins, num, condominio, bloco, andar, unidade, autenticacao, vaga, vaga_locada, modelo, cor, descricao, foto, observacao, marca, placa_letras, placa_numeros, autenticacao_visual, bloqueio, portas, controle_porteiro, tipo_veiculo, situacao_estacionamento, lastdate, lasttime, lastuser, p1_data, p1_hora, p2_data, p2_hora, p4_data, p4_hora, proprietario, serial, new_num
             from condominio.veiculos
            where num notnull and autenticacao notnull and num = $_$ || downloaditem.num || $_$
           order by num desc
      $_$) AS r( filedate date, timerg time, uidins varchar, num serial, condominio integer, bloco integer, andar integer, unidade integer, autenticacao varchar,
           vaga integer, vaga_locada numeric, modelo varchar, cor varchar, descricao varchar, foto varchar, observacao varchar, marca varchar, placa_letras varchar,
           placa_numeros varchar, autenticacao_visual varchar, bloqueio numeric, portas character varying[], controle_porteiro integer, tipo_veiculo integer,
           situacao_estacionamento numeric, lastdate date, lasttime time, lastuser varchar, p1_data date, p1_hora time, p2_data date, p2_hora time, p4_data date,
           p4_hora time, proprietario varchar, serial varchar, new_num integer);

      IF (item.condominio NOTNULL) THEN
          raise notice '%', item.num;
          INSERT INTO veiculos (filedate, timerg, uidins, num, condominio, bloco, andar, unidade, autenticacao, vaga, vaga_locada, modelo, cor, descricao, foto, observacao, marca, placa_letras, placa_numeros, autenticacao_visual, bloqueio, portas, controle_porteiro, tipo_veiculo, situacao_estacionamento, lastdate, lasttime, lastuser, p1_data, p1_hora, p2_data, p2_hora, p4_data, p4_hora, proprietario, serial, new_num)
          VALUES (item.filedate, item.timerg, item.uidins, item.num, item.condominio, item.bloco, item.andar, item.unidade, item.autenticacao, item.vaga, item.vaga_locada, item.modelo, item.cor, item.descricao, item.foto, item.observacao, item.marca, item.placa_letras, item.placa_numeros, item.autenticacao_visual, item.bloqueio, item.portas, item.controle_porteiro, item.tipo_veiculo, item.situacao_estacionamento, item.lastdate, item.lasttime, item.lastuser, item.p1_data, item.p1_hora, item.p2_data, item.p2_hora, item.p4_data, item.p4_hora, item.proprietario, item.serial, item.new_num);
      ELSE
        RAISE NOTICE ' % Nao foi importado', downloaditem.num;
      END IF;

      EXIT WHEN linhalimite = limite;
      linhalimite = linhalimite + 1;

  END LOOP;

  /* Final do procedimento de download */

  /* Inicio do procedimento de limpeza dos registros que foram removidos no banco de dados central */

  DELETE
    FROM veiculos
   WHERE num in (
     SELECT veiculos.num as veiculo
       FROM chaves_veiculos
      RIGHT JOIN veiculos on chaves_veiculos.num = veiculos.num
      WHERE chaves_veiculos.num isnull
   );

  /* Final do procedimento de limpeza dos registros que foram removidos no banco de dados central */

  SELECT count(*)
    INTO totalregistros
    FROM veiculos;

  PERFORM dblink_disconnect();
  RETURN totalregistros;
END;
$_X$;


--
-- TOC entry 263 (class 1255 OID 44479)
-- Name: atualizarcadastros_moradores(character varying, bigint); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.atualizarcadastros_moradores(origem character varying, limite bigint) RETURNS integer
    LANGUAGE plpgsql
    AS $_X$
DECLARE
  totalregistros int;
  downloaditem   record;
  item           record;
  linhalimite int;
BEGIN

  totalregistros := 0;
  linhalimite := 0;

  -- Inicia a conexao com o banco de dados central
  PERFORM dblink_connect(origem);

  /* Inicio do procedimento de download */

  -- Remove as chaves de comparacao
  TRUNCATE TABLE chaves_moradores;

  -- Cria uma tabela temporária para armazenar as matriculas
  INSERT INTO chaves_moradores
  SELECT num
    FROM dblink($_$
         SELECT num
           FROM condominio.moradores
          WHERE num notnull and foto1 notnull and autenticacao notnull
    $_$) AS t(num int);

  -- Compara e obtem as matriculas que ainda não existem na tabela de pedestre
  FOR downloaditem IN
      SELECT chaves_moradores.num
        FROM chaves_moradores
        LEFT JOIN pedestre ON chaves_moradores.num = pedestre.num
       WHERE pedestre.num ISNULL
  LOOP

    SELECT *
      INTO item
      FROM dblink($_$
         select condominio, bloco, andar, unidade, bloqueio, num, foto1, autenticacao, nome, 'Morador' as tipo_de_cadastro, ativacao, substring(nascimento, 1, 10) as nascimento
           from condominio.moradores
           where num notnull and foto1 notnull and autenticacao notnull and num = $_$ || downloaditem.num || $_$
           order by num desc
      $_$) AS r( condominio integer, bloco integer, andar integer, unidade integer, bloqueio numeric, num integer, foto1 varchar, autenticacao varchar(10), nome varchar(400), tipo_de_cadastro varchar(50), ativacao timestamp, nascimento varchar(10));

      IF (item.condominio NOTNULL) THEN
          raise notice '%', item.num;
          INSERT INTO pedestre (condominio, bloco, andar, unidade, bloqueio, num, foto1, autenticacao, nome, tipo_de_cadastro, ativacao, nascimento)
          VALUES (item.condominio, item.bloco, item.andar, item.unidade, item.bloqueio, item.num, item.foto1, item.autenticacao, item.nome, item.tipo_de_cadastro, item.ativacao, item.nascimento);
      ELSE
        RAISE NOTICE ' % Nao foi importado', downloaditem.num;
      END IF;

      EXIT WHEN linhalimite = limite;
      linhalimite = linhalimite + 1;

  END LOOP;

  /* Final do procedimento de download */

  /* Inicio do procedimento de limpeza dos registros que foram removidos no banco de dados central */

  DELETE
    FROM pedestre
   WHERE num in (
     SELECT pedestre.num as pedestre
       FROM chaves_moradores
      RIGHT JOIN pedestre on chaves_moradores.num = pedestre.num
      WHERE chaves_moradores.num isnull
   );

  /* Final do procedimento de limpeza dos registros que foram removidos no banco de dados central */

  SELECT count(*)
    INTO totalregistros
    FROM pedestre;

  PERFORM dblink_disconnect();
  RETURN totalregistros;
END;
$_X$;


--
-- TOC entry 264 (class 1255 OID 44480)
-- Name: atualizarcadastros_veiculos(character varying, bigint); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.atualizarcadastros_veiculos(origem character varying, limite bigint) RETURNS integer
    LANGUAGE plpgsql
    AS $_X$
DECLARE
  totalregistros int;
  downloaditem   record;
  item           record;
  linhalimite int;
BEGIN

  totalregistros := 0;
  linhalimite := 0;

  -- Inicia a conexao com o banco de dados central
  PERFORM dblink_connect(origem);

  /* Inicio do procedimento de download */

  -- Remove as chaves de comparacao
  TRUNCATE TABLE chaves_veiculos;

  -- Cria uma tabela temporária para armazenar as matriculas
  INSERT INTO chaves_veiculos
    SELECT num
    FROM dblink($_$
         SELECT num
           FROM condominio.veiculos
          WHERE num notnull and autenticacao notnull
    $_$) AS t(num int);

  -- Compara e obtem as matriculas que ainda não existem na tabela
  FOR downloaditem IN
  SELECT chaves_veiculos.num
  FROM chaves_veiculos
    LEFT JOIN veiculos ON chaves_veiculos.num = veiculos.num
  WHERE veiculos.num ISNULL
  LOOP

    SELECT *
    INTO item
    FROM dblink($_$
           select filedate, timerg, uidins, num, condominio, bloco, andar, unidade, autenticacao, vaga, vaga_locada, modelo, cor, descricao, foto, observacao, marca, placa_letras, placa_numeros, autenticacao_visual, bloqueio, portas, controle_porteiro, tipo_veiculo, situacao_estacionamento, lastdate, lasttime, lastuser, p1_data, p1_hora, p2_data, p2_hora, p4_data, p4_hora, proprietario
             from condominio.veiculos
            where num notnull and autenticacao notnull and num = $_$ || downloaditem.num || $_$
           order by num desc
      $_$) AS r( filedate date, timerg time, uidins varchar, num integer, condominio integer, bloco integer, andar integer, unidade integer, autenticacao varchar,
         vaga integer, vaga_locada numeric, modelo varchar, cor varchar, descricao varchar, foto varchar, observacao varchar, marca varchar, placa_letras varchar,
         placa_numeros varchar, autenticacao_visual varchar, bloqueio numeric, portas character varying[], controle_porteiro integer, tipo_veiculo integer,
         situacao_estacionamento numeric, lastdate date, lasttime time, lastuser varchar, p1_data date, p1_hora time, p2_data date, p2_hora time, p4_data date,
         p4_hora time, proprietario varchar);

    IF (item.condominio NOTNULL) THEN
      raise notice '%', item.num;
      INSERT INTO veiculos (filedate, timerg, uidins, num, condominio, bloco, andar, unidade, autenticacao, vaga, vaga_locada, modelo, cor, descricao, foto, observacao, marca, placa_letras, placa_numeros, autenticacao_visual, bloqueio, portas, controle_porteiro, tipo_veiculo, situacao_estacionamento, lastdate, lasttime, lastuser, p1_data, p1_hora, p2_data, p2_hora, p4_data, p4_hora, proprietario)
      VALUES (item.filedate, item.timerg, item.uidins, item.num, item.condominio, item.bloco, item.andar, item.unidade, item.autenticacao, item.vaga, item.vaga_locada, item.modelo, item.cor, item.descricao, item.foto, item.observacao, item.marca, item.placa_letras, item.placa_numeros, item.autenticacao_visual, item.bloqueio, item.portas, item.controle_porteiro, item.tipo_veiculo, item.situacao_estacionamento, item.lastdate, item.lasttime, item.lastuser, item.p1_data, item.p1_hora, item.p2_data, item.p2_hora, item.p4_data, item.p4_hora, item.proprietario);
    ELSE
      RAISE NOTICE ' % Nao foi importado', downloaditem.num;
    END IF;

    EXIT WHEN linhalimite = limite;
    linhalimite = linhalimite + 1;

  END LOOP;

  /* Final do procedimento de download */

  /* Inicio do procedimento de limpeza dos registros que foram removidos no banco de dados central */

  DELETE
  FROM veiculos
  WHERE num in (
    SELECT veiculos.num as veiculo
    FROM chaves_veiculos
      RIGHT JOIN veiculos on chaves_veiculos.num = veiculos.num
    WHERE chaves_veiculos.num isnull
  );

  /* Final do procedimento de limpeza dos registros que foram removidos no banco de dados central */

  SELECT count(*)
  INTO totalregistros
  FROM veiculos;

  PERFORM dblink_disconnect();
  RETURN totalregistros;
END;
$_X$;


--
-- TOC entry 261 (class 1255 OID 30800)
-- Name: atualizarpassagens(character varying); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.atualizarpassagens(origem character varying) RETURNS void
    LANGUAGE plpgsql
    AS $_X$
DECLARE

BEGIN
  -- Inicia a conexao com o banco de dados central
  PERFORM dblink_connect(origem);

  /* Inicio do procedimento de upload das autorizações de acesso */
  DECLARE
    passagens RECORD;
  BEGIN
    FOR passagens IN SELECT * FROM passagem_pedestre LOOP
      PERFORM dblink_exec($_$
        INSERT INTO acesso.passagem_pedestre (filedate, timerg, uidins, num, condominio, bloco, andar, unidade, autenticacao, bloqueio, nome, equipamento, localizacao_equipamento, leitor, localizacao_leitor, situacao_leitor, situacao_usuario, dsc_situacao, sentido, tipo_de_cadastro, dispositivo)
        VALUES (
          '$_$ || passagens.filedate || $_$',
          '$_$ || passagens.timerg || $_$',
          '$_$ || passagens.uidins || $_$',
          '$_$ || passagens.num || $_$',
          '$_$ || passagens.condominio || $_$',
          '$_$ || passagens.bloco || $_$',
          '$_$ || passagens.andar || $_$',
          '$_$ || passagens.unidade || $_$',
          '$_$ || passagens.autenticacao || $_$',
          '$_$ || passagens.bloqueio || $_$',
          '$_$ || passagens.nome || $_$',
          '$_$ || passagens.equipamento || $_$',
          '$_$ || passagens.localizacao_equipamento || $_$',
          '$_$ || passagens.leitor || $_$',
          '$_$ || passagens.localizacao_leitor || $_$',
          '$_$ || passagens.situacao_leitor || $_$',
          '$_$ || passagens.situacao_usuario || $_$',
          '$_$ || passagens.dsc_situacao || $_$',
          '$_$ || passagens.sentido || $_$',
          '$_$ || passagens.tipo_de_cadastro || $_$',
          '$_$ || passagens.dispositivo || $_$'
        )
      $_$);

      DELETE
        FROM passagem_pedestre
       WHERE num = passagens.num;
    END LOOP;
  END;

  /* Final do procedimento de upload das autorizações de acesso */
  PERFORM dblink_disconnect();
  RETURN;
END;
$_X$;


--
-- TOC entry 265 (class 1255 OID 40688)
-- Name: buscarpedestre(json); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.buscarpedestre(info json) RETURNS SETOF json
    LANGUAGE plpgsql
    AS $$
DECLARE
  usuario RECORD;
  serialid varchar;
BEGIN

  serialid := RIGHT(upper((info->>'serial')), 5);

  SELECT pedestre.condominio, unidades.bloco, unidades.andar, unidades.unidade, autenticacao, primeironome(nome) AS nome, bloqueio
    INTO usuario
    FROM public.pedestre
    JOIN public.unidades ON pedestre.bloco = unidades.bloco and pedestre.unidade = unidades.num
   WHERE autenticacao = serialid;

  IF (usuario.nome ISNULL) THEN
    RETURN;
  END IF;

  INSERT INTO public.passagem_pedestre (condominio, bloco, andar, unidade, autenticacao, bloqueio, equipamento, localizacao_equipamento, leitor, localizacao_leitor, situacao_leitor, situacao_usuario, dsc_situacao, sentido, nome, tipo_de_cadastro, dispositivo)
  VALUES (usuario.condominio, usuario.bloco, usuario.andar, usuario.unidade, usuario.autenticacao, usuario.bloqueio, (info ->> 'equipamento')::int, (info ->> 'localizacao_equipamento'), (info ->> 'leitor')::int, (info ->> 'localizacao_leitor'), 'Autorizado', 'Autorizado', 'Autorizado', (info ->> 'sentido'), usuario.nome, 'Morador', 'RFID');

  RETURN QUERY
  SELECT array_to_json(array_agg(row_to_json(usuario_info))) AS info
  FROM (
         SELECT
           pedestre.condominio, unidades.bloco, unidades.andar, unidades.unidade,
           primeironome(nome) AS nome,
           idade(nascimento)  as idade,
           foto1,
           CASE WHEN autenticacao = serialid
             THEN 1
           ELSE 0 END         AS usuario_identificado,
           autenticacao,
           bloqueio
         FROM public.pedestre
           JOIN public.unidades ON pedestre.bloco = unidades.bloco and pedestre.unidade = unidades.num
         WHERE usuario.bloco = unidades.bloco and usuario.unidade = unidades.unidade
       ) AS usuario_info;

END;
$$;


--
-- TOC entry 266 (class 1255 OID 44860)
-- Name: buscarveiculo(json); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.buscarveiculo(info json) RETURNS SETOF json
    LANGUAGE plpgsql
    AS $$
DECLARE
  veiculo RECORD;
  serialid varchar;
BEGIN

  serialid := RIGHT(upper((info->>'serial')), 6);

  SELECT veiculos.condominio, unidades.bloco, unidades.andar, unidades.unidade, autenticacao, bloqueio, modelo, placa_letras, placa_numeros, cor
    INTO veiculo
    FROM public.veiculos
    JOIN public.unidades ON veiculos.bloco = unidades.bloco and veiculos.unidade = unidades.num
   WHERE autenticacao = serialid;

  IF (veiculo.modelo ISNULL) THEN
    RETURN;
  END IF;

  INSERT INTO public.passagem_veiculo (condominio, bloco, andar, unidade, autenticacao, bloqueio, equipamento, localizacao_equipamento, leitor, localizacao_leitor, situacao_leitor, situacao_usuario, dsc_situacao, sentido, modelo, placa_letras, placa_numeros, cor, dispositivo)
  VALUES (veiculo.condominio, veiculo.bloco, veiculo.andar, veiculo.unidade, veiculo.autenticacao, veiculo.bloqueio, (info ->> 'equipamento')::int, (info ->> 'localizacao_equipamento'),
        (info ->> 'leitor')::int, (info ->> 'localizacao_leitor'), 'Autorizado', 'Autorizado', 'Autorizado', (info ->> 'sentido'), veiculo.modelo, veiculo.placa_letras, veiculo.placa_numeros, veiculo.cor, 'TAG');

  RETURN QUERY
  SELECT array_to_json(array_agg(row_to_json(veiculos_info))) AS info
  FROM (
         SELECT unidades.bloco, unidades.unidade, modelo, placa_letras || '-' || placa_numeros as placa, cor
         FROM public.veiculos
         JOIN public.unidades ON veiculos.bloco = unidades.bloco and veiculos.unidade = unidades.num
         WHERE autenticacao = serialid
       ) AS veiculos_info;

END;
$$;


--
-- TOC entry 238 (class 1255 OID 20956)
-- Name: idade(character varying); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.idade(nascimento character varying) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE

  quantos_anos    INTEGER;
  ano_atual       INTEGER;
  mes_atual       INTEGER;
  dia_atual       INTEGER;

  ano_aniversario INTEGER;
  mes_aniversario INTEGER;
  dia_aniversario INTEGER;
  data_nascimento DATE;
  
  numeroidade varchar;

BEGIN

  numeroidade := regexp_replace(nascimento , '[^0-9]*', '', 'g');
  IF (length(numeroidade) = 8) THEN
    data_nascimento := to_date(numeroidade, 'DDMMYYYY');
  ELSE
    data_nascimento = current_date;
  END IF;
  
  ano_atual := extract(YEAR FROM current_date);
  mes_atual := extract(MONTH FROM current_date);
  dia_atual := extract(DAY FROM current_date);

  ano_aniversario = extract(YEAR FROM data_nascimento);
  mes_aniversario = extract(MONTH FROM data_nascimento);
  dia_aniversario = extract(DAY FROM data_nascimento);

  quantos_anos = ano_atual - ano_aniversario;

  IF (mes_atual <= mes_aniversario)
  THEN
    quantos_anos = quantos_anos - 1;
  END IF;

  IF (quantos_anos < 0)
  THEN
    RETURN 0;
  ELSE
    RETURN quantos_anos;
  END IF;

END;
$$;


--
-- TOC entry 206 (class 1255 OID 20955)
-- Name: primeironome(character varying); Type: FUNCTION; Schema: public; Owner: -
--

CREATE FUNCTION public.primeironome(nome character varying) RETURNS character varying
    LANGUAGE plpgsql
    AS $$
BEGIN
  IF (position(' ' IN nome) = 0)
  THEN
    RETURN nome;
  END IF;
  RETURN SUBSTRING(nome, 1, position(' ' IN nome) - 1);
END;
$$;


SET default_tablespace = '';

SET default_with_oids = false;

--
-- TOC entry 201 (class 1259 OID 40117)
-- Name: chaves_moradores; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.chaves_moradores (
    num bigint NOT NULL
);


--
-- TOC entry 204 (class 1259 OID 44295)
-- Name: chaves_veiculos; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.chaves_veiculos (
    num bigint NOT NULL
);


--
-- TOC entry 198 (class 1259 OID 30457)
-- Name: passagem_pedestre; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.passagem_pedestre (
    filedate date DEFAULT ('now'::text)::date NOT NULL,
    timerg time without time zone DEFAULT ('now'::text)::time with time zone NOT NULL,
    uidins character varying DEFAULT "current_user"() NOT NULL,
    num integer NOT NULL,
    condominio integer,
    bloco integer,
    andar integer,
    unidade integer,
    autenticacao character varying,
    bloqueio numeric,
    equipamento integer,
    localizacao_equipamento character varying,
    leitor integer,
    localizacao_leitor character varying,
    situacao_leitor character varying,
    situacao_usuario character varying,
    dsc_situacao character varying,
    sentido character varying,
    nome character varying,
    tipo_de_cadastro character varying,
    dispositivo character varying
);


--
-- TOC entry 197 (class 1259 OID 30455)
-- Name: passagem_pedestre_num_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.passagem_pedestre_num_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 2276 (class 0 OID 0)
-- Dependencies: 197
-- Name: passagem_pedestre_num_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.passagem_pedestre_num_seq OWNED BY public.passagem_pedestre.num;


--
-- TOC entry 200 (class 1259 OID 30471)
-- Name: passagem_veiculo; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.passagem_veiculo (
    filedate date DEFAULT ('now'::text)::date NOT NULL,
    timerg time without time zone DEFAULT ('now'::text)::time with time zone NOT NULL,
    uidins character varying DEFAULT "current_user"() NOT NULL,
    num integer NOT NULL,
    condominio integer,
    bloco integer,
    andar integer,
    unidade integer,
    autenticacao character varying,
    bloqueio numeric,
    equipamento integer,
    localizacao_equipamento character varying,
    leitor integer,
    localizacao_leitor character varying,
    situacao_leitor character varying,
    situacao_usuario character varying,
    dsc_situacao character varying,
    sentido character varying,
    modelo character varying,
    placa_letras character varying,
    placa_numeros character varying,
    cor character varying,
    dispositivo character varying,
    acesso_supervisionado character varying
);


--
-- TOC entry 199 (class 1259 OID 30469)
-- Name: passagem_veiculo_num_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.passagem_veiculo_num_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 2277 (class 0 OID 0)
-- Dependencies: 199
-- Name: passagem_veiculo_num_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.passagem_veiculo_num_seq OWNED BY public.passagem_veiculo.num;


--
-- TOC entry 196 (class 1259 OID 29912)
-- Name: pedestre; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.pedestre (
    condominio integer NOT NULL,
    bloco integer NOT NULL,
    andar integer NOT NULL,
    unidade integer NOT NULL,
    bloqueio numeric DEFAULT 0,
    num integer NOT NULL,
    foto1 character varying,
    autenticacao character varying(10) NOT NULL,
    nome character varying(400) NOT NULL,
    tipo_de_cadastro character varying(50) NOT NULL,
    ativacao timestamp without time zone,
    nascimento character varying(10) NOT NULL
);


--
-- TOC entry 205 (class 1259 OID 44850)
-- Name: unidades; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.unidades (
    filedate date,
    timerg time without time zone,
    uidins character varying,
    num integer,
    condominio integer,
    bloco integer,
    andar integer,
    unidade integer,
    situacao numeric,
    imobiliaria character varying,
    telefone_imobiliaria character varying,
    nome_proprietario character varying,
    telefone_proprietario character varying,
    email_proprietario character varying,
    rg character varying,
    telefone_proprietario_imobiliaria character varying,
    email_proprietario_imobiliaria character varying,
    lastdate date,
    lasttime time without time zone,
    lastuser character varying,
    vaga1 integer,
    vaga2 integer,
    vaga3 integer,
    vaga4 integer,
    vaga5 integer,
    vaga6 integer,
    vaga7 integer,
    vaga8 integer,
    vaga9 integer,
    vaga10 integer,
    email_correspondencias character varying,
    email_correspondencias2 character varying,
    entrada_data_inicial date,
    entrada_data_final date,
    saida_data_inicial date,
    saida_data_final date,
    reforma_data_incial date,
    reforma_data_final date,
    ativacao_data date,
    ativacao_hora time without time zone
);


--
-- TOC entry 203 (class 1259 OID 44270)
-- Name: veiculos; Type: TABLE; Schema: public; Owner: -
--

CREATE TABLE public.veiculos (
    filedate date DEFAULT ('now'::text)::date NOT NULL,
    timerg time without time zone DEFAULT ('now'::text)::time with time zone NOT NULL,
    uidins character varying DEFAULT "current_user"() NOT NULL,
    num integer NOT NULL,
    condominio integer NOT NULL,
    bloco integer NOT NULL,
    andar integer NOT NULL,
    unidade integer NOT NULL,
    autenticacao character varying,
    vaga integer,
    vaga_locada numeric,
    modelo character varying,
    cor character varying,
    descricao character varying,
    foto character varying,
    observacao character varying,
    marca character varying,
    placa_letras character varying(3),
    placa_numeros character varying,
    autenticacao_visual character varying,
    bloqueio numeric(2,0),
    portas character varying[] DEFAULT '{1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,80,81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,96,97,98,99}'::character varying[],
    controle_porteiro integer,
    tipo_veiculo integer DEFAULT 1 NOT NULL,
    situacao_estacionamento numeric(1,0) DEFAULT 0 NOT NULL,
    lastdate date,
    lasttime time without time zone,
    lastuser character varying,
    p1_data date,
    p1_hora time without time zone,
    p2_data date,
    p2_hora time without time zone,
    p4_data date,
    p4_hora time without time zone,
    proprietario character varying DEFAULT 'morador'::character varying NOT NULL,
    serial character varying,
    new_num integer
);


--
-- TOC entry 2278 (class 0 OID 0)
-- Dependencies: 203
-- Name: COLUMN veiculos.filedate; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.veiculos.filedate IS 'data corrente';


--
-- TOC entry 2279 (class 0 OID 0)
-- Dependencies: 203
-- Name: COLUMN veiculos.timerg; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.veiculos.timerg IS 'hora corrente';


--
-- TOC entry 2280 (class 0 OID 0)
-- Dependencies: 203
-- Name: COLUMN veiculos.uidins; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.veiculos.uidins IS 'usuario corrente';


--
-- TOC entry 2281 (class 0 OID 0)
-- Dependencies: 203
-- Name: COLUMN veiculos.num; Type: COMMENT; Schema: public; Owner: -
--

COMMENT ON COLUMN public.veiculos.num IS 'numero de identificacao do andar';


--
-- TOC entry 202 (class 1259 OID 44268)
-- Name: veiculos_num_seq; Type: SEQUENCE; Schema: public; Owner: -
--

CREATE SEQUENCE public.veiculos_num_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


--
-- TOC entry 2282 (class 0 OID 0)
-- Dependencies: 202
-- Name: veiculos_num_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: -
--

ALTER SEQUENCE public.veiculos_num_seq OWNED BY public.veiculos.num;


--
-- TOC entry 2114 (class 2604 OID 30463)
-- Name: passagem_pedestre num; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.passagem_pedestre ALTER COLUMN num SET DEFAULT nextval('public.passagem_pedestre_num_seq'::regclass);


--
-- TOC entry 2118 (class 2604 OID 30477)
-- Name: passagem_veiculo num; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.passagem_veiculo ALTER COLUMN num SET DEFAULT nextval('public.passagem_veiculo_num_seq'::regclass);


--
-- TOC entry 2122 (class 2604 OID 44276)
-- Name: veiculos num; Type: DEFAULT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.veiculos ALTER COLUMN num SET DEFAULT nextval('public.veiculos_num_seq'::regclass);


--
-- TOC entry 2135 (class 2606 OID 40121)
-- Name: chaves_moradores chaves_moradores_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.chaves_moradores
    ADD CONSTRAINT chaves_moradores_pkey PRIMARY KEY (num);


--
-- TOC entry 2144 (class 2606 OID 44299)
-- Name: chaves_veiculos chaves_veiculos_pkey; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.chaves_veiculos
    ADD CONSTRAINT chaves_veiculos_pkey PRIMARY KEY (num);


--
-- TOC entry 2132 (class 2606 OID 30482)
-- Name: passagem_veiculo primarykey_acesso_passagem_veiculo; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.passagem_veiculo
    ADD CONSTRAINT primarykey_acesso_passagem_veiculo PRIMARY KEY (num);


--
-- TOC entry 2141 (class 2606 OID 44285)
-- Name: veiculos primarykey_condominio_veiculos; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.veiculos
    ADD CONSTRAINT primarykey_condominio_veiculos PRIMARY KEY (num, unidade, andar, bloco, condominio);


--
-- TOC entry 2128 (class 2606 OID 29919)
-- Name: pedestre primarykey_morador; Type: CONSTRAINT; Schema: public; Owner: -
--

ALTER TABLE ONLY public.pedestre
    ADD CONSTRAINT primarykey_morador PRIMARY KEY (autenticacao);


--
-- TOC entry 2133 (class 1259 OID 40122)
-- Name: chaves_moradores_num_uindex; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX chaves_moradores_num_uindex ON public.chaves_moradores USING btree (num);


--
-- TOC entry 2142 (class 1259 OID 44300)
-- Name: chaves_veiculos_num_uindex; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX chaves_veiculos_num_uindex ON public.chaves_veiculos USING btree (num);


--
-- TOC entry 2136 (class 1259 OID 44291)
-- Name: fki_foreignkey_condominio_veiculos_unidade; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX fki_foreignkey_condominio_veiculos_unidade ON public.veiculos USING btree (unidade);


--
-- TOC entry 2137 (class 1259 OID 44292)
-- Name: fki_foreignkey_veiculos_unidades; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX fki_foreignkey_veiculos_unidades ON public.veiculos USING btree (condominio, bloco, andar, unidade);


--
-- TOC entry 2129 (class 1259 OID 30467)
-- Name: index_acesso_passagem_pedestre; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX index_acesso_passagem_pedestre ON public.passagem_pedestre USING btree (num);


--
-- TOC entry 2130 (class 1259 OID 30483)
-- Name: index_acesso_passagem_veiculo; Type: INDEX; Schema: public; Owner: -
--

CREATE INDEX index_acesso_passagem_veiculo ON public.passagem_veiculo USING btree (num);


--
-- TOC entry 2138 (class 1259 OID 44293)
-- Name: index_veiculos_autenticacao; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX index_veiculos_autenticacao ON public.veiculos USING btree (autenticacao) WHERE ((autenticacao IS NOT NULL) AND ((autenticacao)::text <> '0'::text));


--
-- TOC entry 2139 (class 1259 OID 44294)
-- Name: index_veiculos_serial; Type: INDEX; Schema: public; Owner: -
--

CREATE UNIQUE INDEX index_veiculos_serial ON public.veiculos USING btree (serial DESC);


--
-- TOC entry 2266 (class 2618 OID 30260)
-- Name: pedestre ignore_duplicate_inserts_on_t; Type: RULE; Schema: public; Owner: -
--

CREATE RULE ignore_duplicate_inserts_on_t AS
    ON INSERT TO public.pedestre
   WHERE (EXISTS ( SELECT 1
           FROM public.pedestre
          WHERE ((pedestre.autenticacao)::text = (new.autenticacao)::text))) DO INSTEAD NOTHING;


-- Completed on 2019-02-05 10:40:21

--
-- PostgreSQL database dump complete
--

