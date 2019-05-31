using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;


namespace linear.core
{

    public class controladoraLinear
    {

        private static List<KeyValuePair<int, string>> _tiporetorno;

        public static List<KeyValuePair<int, string>> TipoRetorno
        {
            get
            {
                return _tiporetorno;
            }
        }

        public MemoriaPlaca memoriaPlaca;

        //--------------------------------------------------------------
        //                          CONSTRUTOR
        //--------------------------------------------------------------
        public controladoraLinear()
            : base()
        {
            _tiporetorno = new List<KeyValuePair<int, string>>()
            {
                    new KeyValuePair<int, string>(0, "RT_OK"),
                    new KeyValuePair<int, string>(1, "RT_MEMORIA_CHEIA"),
                    new KeyValuePair<int, string>(2, "RT_DISP_JA_APRENDIDO"),
                    new KeyValuePair<int, string>(3, "RT_DISP_NAO_ENCONTRADO"),
                    new KeyValuePair<int, string>(4, "RT_SEM_MAIS_EVENTOS"),
                    new KeyValuePair<int, string>(5, "RT_FIM_DO_ARQUIVO"),
                    new KeyValuePair<int, string>(6, "RT_ESTOURO_BUFFER_INTERNO"),
                    new KeyValuePair<int, string>(7, "RT_CONFLITO_DE_PORTAS"),
                    new KeyValuePair<int, string>(8, "RT_ERRO_NO_TAMANHO_PACOTE"),
                    new KeyValuePair<int, string>(9, "RT_ERRO_DE_CHECKSUM"),
                    new KeyValuePair<int, string>(10, "RT_ERRO_NO_MODO_OPERACAO"),
                    new KeyValuePair<int, string>(11, "RT_ERRO_ESCRITA_LEITURA"),
                    new KeyValuePair<int, string>(12, "RT_ERRO_FORA_DO_LIMITE_DO_DISPOSITIVO"),
                    new KeyValuePair<int, string>(13, "RT_ERRO_TIPO_DIFERENTE"),
                    new KeyValuePair<int, string>(14, "RT_ERRO_OPERACAO_INVALIDA"),
                    new KeyValuePair<int, string>(15, "RT_ERRO_DISPOSITIVO_DIFERENTE"),
                    new KeyValuePair<int, string>(16, "RT_ERRO_INDICE_INVALIDO"),
                    new KeyValuePair<int, string>(17, "RT_ERRO_DE_SINTAXE"),
                    new KeyValuePair<int, string>(18, "RT_ERRO_VALOR_INVALIDO"),
                    new KeyValuePair<int, string>(19, "RT_ERRO_SERIAL_INVALIDO"),
                    new KeyValuePair<int, string>(20, "RT_ERRO_SETOR_DIFERENTE"),
                    new KeyValuePair<int, string>(21, "RT_ERRO_ENDERECO_DIFERENTE"),
                    new KeyValuePair<int, string>(22, "RT_ERRO_BIOMETRIA_1_SUCESSO_BIOMETRIA_2"),
                    new KeyValuePair<int, string>(23, "RT_ERRO_BIOMETRIA_2_SUCESSO_BIOMETRIA_1"),
                    new KeyValuePair<int, string>(24, "RT_ERRO_BIOMETRIA_1_E_2"),
                    new KeyValuePair<int, string>(25, "RT_ERRO_SEM_RESPOSTA_DO_MODULO"),
                    new KeyValuePair<int, string>(244, "RT_ERRO_FECHAR_ARQUIVO"),
                    new KeyValuePair<int, string>(245, "RT_ERRO_MIDIA_LEITURA"),
                    new KeyValuePair<int, string>(246, "RT_ERRO_MIDIA_ESCRITA"),
                    new KeyValuePair<int, string>(247, "RT_ERRO_SEM_ARQUIVO_ABERTO"),
                    new KeyValuePair<int, string>(248, "RT_ERRO_ARQUIVO_ABERTO"),
                    new KeyValuePair<int, string>(249, "RT_ERRO_EOF"),
                    new KeyValuePair<int, string>(250, "RT_ERRO_MIDIA"),
                    new KeyValuePair<int, string>(251, "RT_ERRO_TIMEOUT"),
                    new KeyValuePair<int, string>(252, "RT_ERRO_ARQUIVO"),
                    new KeyValuePair<int, string>(253, "RT_ERRO_PASTA"),
                    new KeyValuePair<int, string>(254, "RT_ERRO_SD"),
                    new KeyValuePair<int, string>(255, "RT_ERRO")
            };
        }

        //--------------------------------------------------------------
        //                ESTRUTURAS E VARIAVEIS GLOBAIS
        //--------------------------------------------------------------


        public const byte
                        N_ENDERECOS = 64,
                        SIZE_OF_BYTES_RESERVADOS = 20,
                        N_LEITORAS = 4,
                        SIZE_OF_DDNS_USUARIO = 16,
                        SIZE_OF_DDNS_SENHA = 16,
                        SIZE_OF_DDNS_DEVICE = 40,
                        SIZE_OF_USUARIO_LOGIN = 16,
                        SIZE_OF_SENHA_LOGIN = 16,
                        SIZE_OF_DNS_HOST = 16,
                        SIZE_OF_DDNS_HOST = 40;

        public const byte
                        //---------------------------------------------------------------------------------------
                        // modo de operação
                        //---------------------------------------------------------------------------------------
                        MODO_CATRACA = 0,
                        MODO_CTRL_PORTA = 1,
                        MODO_CTRL_CANCELA = 2,
                        MODO_TESTE = 3;

        public const byte
                        //---------------------------------------------------------------------------------------
                        // autonomia
                        //---------------------------------------------------------------------------------------
                        AUTONOMIA_LOCAL = 0,
                        AUTONOMIA_REMOTO = 1,
                        AUTONOMIA_TEMPORIZADO = 2;

        public const byte
                        //---------------------------------------------------------------------------------------
                        // sinalização das saídas digitais
                        //---------------------------------------------------------------------------------------
                        SINALIZA_ABRIU = 0,
                        SINALIZA_FECHOU = 1,
                        SINALIZA_FAROL = 2;

        public const byte
                        //---------------------------------------------------------------------------------------
                        // logs (eventos)
                        //---------------------------------------------------------------------------------------
                        LOG_DISP_ACIONADO = 0,
                        LOG_PASSAGEM = 1,
                        LOG_LIGADO = 2,
                        LOG_DESP_PORT = 3,
                        LOG_MUDANCA_PROG = 4,
                        LOG_VAGO_5 = 5,
                        LOG_ACIONAMENTO_PC = 6,
                        LOG_CONT_ATUALIZACAO = 7,
                        LOG_CLONAGEM = 8,
                        LOG_PANICO = 9,
                        LOG_SDCARD_REMOVIDO = 10,
                        LOG_RESTORE = 11,
                        LOG_ERRO_DE_GRAVACAO = 12,
                        LOG_BACKUP_AUTO = 13,
                        LOG_BACKUP_MANUAL = 14,
                        LOG_SERIAL_NAO_CADASTRADO = 15,
                        LOG_DUPLAPASSAGEM = 16,
                        LOG_NAO_HABILITADO = 17,
                        LOG_BOOTLOADER = 18,
                        LOG_ALARME_ENTRADA_DIGITAL = 19,
                        LOG_RESET_WATCHDOG_TIMER = 20,
                        LOG_ATUALIZACAO_ASSINCRONA = 21,
                        // evento lido
                        LOG_NAO_LIDO = 0,
                        LOG_LIDO = 1,
                        LOG_TODOS = 2;
        public const byte
                        // SUB_LOG_CONT_ATUALIZACAO
                        SUB_LOG_INCONSISTENCIA_ENTRE_BASES_DE_DADOS = 0,
                        // SUB_LOG_DISP_ACIONADO

                        // SUB_LOG_BACKUP_AUTO
                        SUB_LOG_BACKUP_AUTO_OK = 0,
                        SUB_LOG_BACKUP_AUTO_SD_CARD_NAO_ENCONTRADO = 1,
                        SUB_LOG_BACKUP_AUTO_FALHA = 2,
                        // SUB_LOG_BACKUP_MANUAL
                        SUB_LOG_BACKUP_MANUAL_SUCESSO = 0,
                        SUB_LOG_BACKUP_MANUAL_FALHA = 1,
                        // SUB_LOG_PASSAGEM
                        SUB_LOG_PASSAGEM_ENTRADA_1 = 0,
                        SUB_LOG_PASSAGEM_SAIDA_1 = 1,
                        SUB_LOG_PASSAGEM_ENTRADA_2 = 2,
                        SUB_LOG_PASSAGEM_SAIDA_2 = 3,
                        SUB_LOG_PASSAGEM_ENTRADA_3 = 4,
                        SUB_LOG_PASSAGEM_SAIDA_3 = 5,
                        SUB_LOG_PASSAGEM_ENTRADA_4 = 6,
                        SUB_LOG_PASSAGEM_SAIDA_4 = 7,
                        SUB_LOG_PASSAGEM_TOUT = 8,
                        SUB_LOG_PASSAGEM_SAIDA_LIVRE = 9,
                        SUB_LOG_PASSAGEM_BOTAO_1 = 10,
                        SUB_LOG_PASSAGEM_BOTAO_2 = 11,
                        SUB_LOG_PASSAGEM_BOTAO_3 = 12,
                        SUB_LOG_PASSAGEM_BOTAO_4 = 13,
                        SUB_LOG_PASSAGEM_ENTRADA_APB_DESLIGADO = 14,
                        SUB_LOG_PASSAGEM_SAIDA_APB_DESLIGADO = 15,
                        // SUB LOG ACIONAMENTO
                        SUB_LOG_ACIONAMENTO_ENTRADA = 0,
                        SUB_LOG_ACIONAMENTO_SAIDA = 1,
                        SUB_LOG_ACIONAMENTO_BOTAO_1 = 2,
                        SUB_LOG_ACIONAMENTO_BOTAO_2 = 3,
                        SUB_LOG_ACIONAMENTO_BOTAO_3 = 4,
                        SUB_LOG_ACIONAMENTO_BOTAO_4 = 5,
                        SUB_LOG_AGUARDANDO_SEGUNDA_VALIDACAO = 6,
                        // SUB LOG ACIONAMENTO REMOTO
                        SUB_LOG_ACIONAMENTO_REMOTO_OK = 0,
                        SUB_LOG_ACIONAMENTO_REMOTO_ERRO = 1,
                        SUB_LOG_ACIONAMENTO_REMOTO_COM_ID_OK = 2,
                        SUB_LOG_ACIONAMENTO_REMOTO_COM_ID_ERRO = 3,
                        SUB_LOG_ACIONAMENTO_REMOTO_RELE_5 = 4,
                        SUB_LOG_ACIONAMENTO_REMOTO_RELE_6 = 5,
                        // SUB LOG ALARME ENTRADA DIGITAL
                        SUB_LOG_ALARME_ED_1 = 0,
                        SUB_LOG_ALARME_ED_2 = 1,
                        SUB_LOG_ALARME_ED_3 = 2,
                        SUB_LOG_ALARME_ED_4 = 3,
                        SUB_LOG_ALARME_ARROMBAMENTO_1 = 4,
                        SUB_LOG_ALARME_ARROMBAMENTO_2 = 5,
                        SUB_LOG_ALARME_ARROMBAMENTO_3 = 6,
                        SUB_LOG_ALARME_ARROMBAMENTO_4 = 7,
                        // SUB LOG DUPLA PASSAGEM
                        SUB_LOG_DUPLA_PASSAGEM_ENTRADA_1 = 0,
                        SUB_LOG_DUPLA_PASSAGEM_SAIDA_1 = 1,
                        SUB_LOG_DUPLA_PASSAGEM_ENTRADA_2 = 2,
                        SUB_LOG_DUPLA_PASSAGEM_SAIDA_2 = 3,
                        SUB_LOG_DUPLA_PASSAGEM_ENTRADA_3 = 4,
                        SUB_LOG_DUPLA_PASSAGEM_SAIDA_3 = 5,
                        SUB_LOG_DUPLA_PASSAGEM_ENTRADA_4 = 6,
                        SUB_LOG_DUPLA_PASSAGEM_SAIDA_4 = 7,
                         // SUB LOG NAO CADASTRADO
                         SUB_LOG_NAO_CADASTRADO = 1,
                         SUB_LOG_LEITORA_EXPEDIDORA = 2,
                        // LOG_ERRO_DE_GRAVACAO
                        SUB_LOG_ERRO_DE_GRAVACAO_BIOMETRIA_1 = 0,
                        SUB_LOG_ERRO_DE_GRAVACAO_BIOMETRIA_2 = 1,
                        SUB_LOG_ERRO_DE_GRAVACAO_BIOMETRIA_3 = 2,
                        SUB_LOG_ERRO_DE_GRAVACAO_BIOMETRIA_4 = 3,
                        SUB_LOG_ERRO_DE_GRAVACAO_ATUALIZACAO_DE_BIOMETRIA_CANCELADA = 4,
                        // LOG_PANICO
                        SUB_LOG_PANICO = 0,
                        SUB_LOG_PANICO_NAO_ATENDIDO = 1,
                        SUB_LOG_PANICO_CANCELADO = 2,
                        SUB_LOG_PANICO_2X_CARTAO = 3,
                        SUB_LOG_PANICO_IMEDIATO = 4,
                        SUB_LOG_PANICO_TEMPORIZADO = 5,
                        SUB_LOG_PANICO_DISPOSITIVO_PANICO = 6,
                        // LOG_ATUALIZACAO_ASSINCRONA
                        SUB_LOG_ATUALIZACAO_FALHA_DE_GRAVACAO = 0,
                        SUB_LOG_ATUALIZACAO_FALHA_AO_LER_ARQUIVO = 1,
                        SUB_LOG_ATUALIZACAO_CONCLUIDA_COM_SUCESSO = 2,
                        SUB_LOG_ATUALIZACAO_SERIAL_FORA_DO_LIMITE = 3,
                        SUB_LOG_ATUALIZACAO_SEM_BIOMETRIA = 4;


        public const byte
                      LOG_VAGO = 0xFF;
        public const UInt32
                      // LOG
                      LOG_CHEIO = 0xFFFFFFFF,
                      LOG_N_ENCONTRADO = 0x0FFFFFFF,
                      LOG_PONTEIRO = 0x00000055,
                      // DISP
                      DISP_CHEIO = 0xFFFFFFFF,
                      DISP_N_ENCONTRADO = 0x0FFFFFFF,
                      DISP_N_CADASTRADO = 0xEFFFFFFF;

        public const byte
                     DISP_TX = 0x01,
                     DISP_TA = 0x02,
                     DISP_CT = 0x03,
                     DISP_CA = 0x04,
                     DISP_BM = 0x05,
                     DISP_TP = 0x06,
                     DISP_SN = 0x07,
                     DISP_CBS = 0x08,
                     DISP_BT = 0x09,
                     DISP_CM = 0x0B,

                     S_DISP_TX = 0x10,
                     S_DISP_TA = 0x20,
                     S_DISP_CT = 0x30,
                     S_DISP_CA = 0x40,
                     S_DISP_BM = 0x50,
                     S_DISP_TP = 0x60,
                     S_DISP_SN = 0x70,
                     S_DISP_CBS = 0x80,
                     S_DISP_BT = 0x90,
                     S_DISP_CM = 0xB0,

                     DISP_GERAL = 0x0F,
                     DISP_VAGO = 0xF0,
                     DISP_APAGADO = 0xE0;

        public const byte
                    LOG_TIPO_DE_EVENTO = 0,
                    LOG_NUMERO_DO_DISPOSITIVO = 1,
                    LOG_SER_0 = 2,
                    LOG_TIPO_DE_DISPOSITIVO = 2,
                    LOG_SER_1 = 3,
                    LOG_SER_2 = 4,
                    LOG_SER_3 = 5,
                    LOG_SER_4 = 6,
                    LOG_SER_5 = 7,
                    LOG_HORA = 8,
                    LOG_MINUTO = 9,
                    LOG_SEGUNDO = 10,
                    LOG_DIA = 11,
                    LOG_MES = 12,
                    LOG_ANO = 13,
                    LOG_NIVEL = 14,
                    LOG_FLAGS_DE_EVENTO = 15;


        public const int
                        //---------------------------------------------------------------------------------------
                        // controles (32 bytes por controle)
                        //---------------------------------------------------------------------------------------
                        SIZE_OF_DISPOSITIVO = 32,
                        //N_LINES_DISPOSITIVO = 8192,
                        N_LINES_DISPOSITIVO = 32768,
                        DIV_OF_DISPOSITIVO = 32,
                        //---------------------------------------------------------------------------------------
                        // setup
                        //---------------------------------------------------------------------------------------
                        SIZE_OF_SETUP = 384,
                        N_LINES_SETUP = 1,
                        DIV_OF_SETUP = 128,
                        //---------------------------------------------------------------------------------------
                        // tabela de habilitação
                        //---------------------------------------------------------------------------------------
                        SIZE_OF_HABILITACAO = 32,
                        N_LINES_HABILITACAO = 416,
                        DIV_OF_HABILITACAO = 128,
                        //---------------------------------------------------------------------------------------
                        // tabela de grupos das leitoras
                        //---------------------------------------------------------------------------------------
                        SIZE_OF_GRUPO_LEITORA = 256,
                        N_LINES_GRUPO_LEITORA = 416,
                        DIV_OF_GRUPO_LEITORA = 128,
                        //---------------------------------------------------------------------------------------
                        // tabela de jornadas
                        //---------------------------------------------------------------------------------------
                        SIZE_OF_JORNADAS = 8,
                        N_LINES_JORNADAS = 64,
                        DIV_OF_JORNADAS = 128,
                        //---------------------------------------------------------------------------------------
                        // tabela de turnos
                        //---------------------------------------------------------------------------------------
                        SIZE_OF_TURNOS = 16,
                        N_LINES_TURNOS = 128,
                        DIV_OF_TURNOS = 128,
                        //---------------------------------------------------------------------------------------
                        // tabela feriados
                        //---------------------------------------------------------------------------------------
                        SIZE_OF_FERIADO = 2,
                        N_LINES_FERIADO = 24,
                        DIV_OF_FERIADO = 24,
                        //---------------------------------------------------------------------------------------
                        // label das rotas
                        //---------------------------------------------------------------------------------------
                        SIZE_OF_LABEL_ROTAS = 8,
                        N_LINES_LABEL_ROTAS = 416,
                        DIV_OF_LABEL_ROTAS = 128,
                        //---------------------------------------------------------------------------------------
                        // mensagens display externo
                        //---------------------------------------------------------------------------------------
                        SIZE_OF_MSG_DISPLAY = 32,
                        N_LINES_MSG_DISPLAY = 12,
                        DIV_OF_MSG_DISPLAY = 128,
                        //---------------------------------------------------------------------------------------
                        // tabela de direção dos turnos
                        //---------------------------------------------------------------------------------------
                        SIZE_OF_TABELA_DIRECAO_TURNO = 1,
                        N_LINES_TABELA_DIRECAO_TURNO = 128,
                        DIV_OF_DIRECAO_TURNOS = 128,
                        //---------------------------------------------------------------------------------------
                        // tabela de logs (eventos)
                        //---------------------------------------------------------------------------------------
                        SIZE_OF_LOG = 16,
                        N_LINES_LOG = 8192;

        public const int
                          //---------------------------------------------------------------------------------------
                          // COMANDOS
                          //---------------------------------------------------------------------------------------
                          N_BYTES_CMD_79 = 37,
                          N_BYTES_CMD_80 = 36,
                          N_BYTES_CMD_81 = 36,
                          N_BYTES_CMD_82 = 10,
                          N_BYTES_CMD_83 = 3,
                          N_BYTES_CMD_84 = 4,
                          N_BYTES_CMD_85 = 20,
                          N_BYTES_CMD_86 = 35,
                          N_BYTES_CMD_87 = 4,
                          N_BYTES_CMD_88 = 51,
                          N_BYTES_CMD_89 = 4,
                          N_BYTES_CMD_90 = 4,
                          N_BYTES_CMD_91 = 518,
                          N_BYTES_CMD_92 = 3,
                          N_BYTES_CMD_93 = 3,
                          N_BYTES_CMD_94 = 5,
                          N_BYTES_CMD_95 = 3,
                          N_BYTES_CMD_96 = SIZE_OF_SETUP + 3,
                          N_BYTES_CMD_97 = 9,
                          N_BYTES_CMD_98 = 3,
                          N_BYTES_CMD_99 = 518,
                          N_BYTES_CMD_100 = 12,
                          N_BYTES_CMD_101 = 10,
                          N_BYTES_CMD_102 = 6,
                          N_BYTES_CMD_103 = 3,
                          N_BYTES_CMD_104 = 5,
                          N_BYTES_CMD_105 = 4,
                          N_BYTES_CMD_106 = 40,// + template
                          N_BYTES_CMD_107 = 7,
                          N_BYTES_CMD_108 = 7,
                          N_BYTES_CMD_109 = 3,
                          N_BYTES_CMD_110 = 5,
                          N_BYTES_CMD_111 = 4,
                          N_BYTES_CMD_112 = 6,
                          N_BYTES_CMD_113 = 518,
                          N_BYTES_CMD_114 = 12,
                          N_BYTES_CMD_115 = 4,
                          N_BYTES_CMD_116 = 22,
                          N_BYTES_CMD_117 = 6,
                          N_BYTES_CMD_118 = 10,
                          N_BYTES_CMD_119 = 8, // + FRAME COMANDO
                          N_BYTES_CMD_120 = 8, // + FRAME COMANDO
                          N_BYTES_CMD_121 = 16,
                          N_BYTES_CMD_122 = 6,
                          N_BYTES_CMD_123 = 37,
                          N_BYTES_CMD_124 = 518,
                          N_BYTES_CMD_125 = 518,
                          N_BYTES_CMD_126 = 518,
                          N_BYTES_CMD_127 = 33,
                          N_BYTES_CMD_128 = 20,
                          N_BYTES_CMD_129 = 10,
                          N_BYTES_CMD_130 = 3,
                          N_BYTES_CMD_131 = 518,
                          N_BYTES_CMD_132 = 16,
                          N_BYTES_CMD_133 = 6,
                          N_BYTES_CMD_134 = 0,
                          N_BYTES_CMD_135 = 5,
                          N_BYTES_CMD_136 = 11,
                          N_BYTES_CMD_137 = 5,
                          N_BYTES_CMD_138 = 8,
                          N_BYTES_CMD_139 = 5,
                          N_BYTES_CMD_140 = 10,
                          N_BYTES_CMD_141 = 5,

                          N_BYTES_CMD_142 = 6,
                          N_BYTES_CMD_143 = 38,
                          N_BYTES_CMD_144 = 519,
                          N_BYTES_CMD_145 = 7,
                          N_BYTES_CMD_146 = 11,
                          N_BYTES_CMD_147 = 519,
                          N_BYTES_CMD_148 = 10,
                          N_BYTES_CMD_149 = 21,
                          N_BYTES_CMD_150 = 519,
                          N_BYTES_CMD_151 = 61, // + nBytes
                          N_BYTES_CMD_152 = 14,
                          N_BYTES_CMD_153 = 518,
                          N_BYTES_CMD_154 = 6,
                          N_BYTES_CMD_155 = 4,
                          N_BYTES_CMD_156 = 40,// + template
                          N_BYTES_CMD_157 = 21,
                          N_BYTES_CMD_158 = 4,
                          N_BYTES_CMD_159 = 518,
                          N_BYTES_CMD_160 = 16,
                          N_BYTES_CMD_161 = 3,
                          N_BYTES_CMD_162 = 5,

                          //---------------------------------------------------
                          // RESPOSTAS
                          N_BYTES_RSP_CMD_0 = 0,
                          N_BYTES_RSP_CMD_1 = 3,
                          N_BYTES_RSP_CMD_2 = 3,
                          N_BYTES_RSP_CMD_3 = 43,
                          N_BYTES_RSP_CMD_4 = 20,
                          N_BYTES_RSP_CMD_5 = 19,
                          N_BYTES_RSP_CMD_6 = 6,
                          N_BYTES_RSP_CMD_7 = 5,
                          N_BYTES_RSP_CMD_8 = 3,
                          N_BYTES_RSP_CMD_9 = 3,
                          N_BYTES_RSP_CMD_10 = 35,
                          N_BYTES_RSP_CMD_11 = 3,
                          N_BYTES_RSP_CMD_12 = 9,
                          N_BYTES_RSP_CMD_13 = 0,
                          N_BYTES_RSP_CMD_14 = 35,
                          N_BYTES_RSP_CMD_15 = 1,
                          N_BYTES_RSP_CMD_16 = 3,
                          N_BYTES_RSP_CMD_17 = 19,
                          N_BYTES_RSP_CMD_18 = 4,
                          N_BYTES_RSP_CMD_19 = 4,
                          N_BYTES_RSP_CMD_20 = 4,
                          N_BYTES_RSP_CMD_21 = 3,
                          N_BYTES_RSP_CMD_22 = 13,
                          N_BYTES_RSP_CMD_23 = 3,
                          N_BYTES_RSP_CMD_24 = 3,
                          N_BYTES_RSP_CMD_25 = 9,
                          N_BYTES_RSP_CMD_26 = 35,
                          N_BYTES_RSP_CMD_27 = 4,
                          N_BYTES_RSP_CMD_28 = 4,
                          N_BYTES_RSP_CMD_29 = 4,
                          N_BYTES_RSP_CMD_30 = 0,
                          N_BYTES_RSP_CMD_31 = 1075,
                          N_BYTES_RSP_CMD_32 = 1059,
                          N_BYTES_RSP_CMD_33 = 1075,
                          N_BYTES_RSP_CMD_34 = 1075,
                          N_BYTES_RSP_CMD_35 = 3,
                          N_BYTES_RSP_CMD_36 = 1,
                          N_BYTES_RSP_CMD_37 = 35,
                          N_BYTES_RSP_CMD_38 = 8,
                          N_BYTES_RSP_CMD_39 = 0,
                          N_BYTES_RSP_CMD_40 = 0,
                          N_BYTES_RSP_CMD_41 = 0,
                          N_BYTES_RSP_CMD_42 = 11,
                          N_BYTES_RSP_CMD_43 = 0,
                          N_BYTES_RSP_CMD_44 = 263,
                          N_BYTES_RSP_CMD_45 = 5,
                          N_BYTES_RSP_CMD_46 = 10,
                          N_BYTES_RSP_CMD_47 = 5,
                          N_BYTES_RSP_CMD_48 = 5,
                          N_BYTES_RSP_CMD_49 = 5,
                          N_BYTES_RSP_CMD_50 = 0,
                          N_BYTES_RSP_CMD_51 = 0,
                          N_BYTES_RSP_CMD_52 = 0,
                          N_BYTES_RSP_CMD_53 = 0,
                          N_BYTES_RSP_CMD_54 = 0,
                          N_BYTES_RSP_CMD_55 = 0,
                          N_BYTES_RSP_CMD_56 = 0,
                          N_BYTES_RSP_CMD_57 = 0,
                          N_BYTES_RSP_CMD_58 = 0,
                          N_BYTES_RSP_CMD_59 = 6,
                          N_BYTES_RSP_CMD_60 = 0,
                          N_BYTES_RSP_CMD_61 = 10,
                          N_BYTES_RSP_CMD_62 = 0,
                          N_BYTES_RSP_CMD_63 = 0,
                          N_BYTES_RSP_CMD_64 = 3,
                          N_BYTES_RSP_CMD_65 = 0,
                          N_BYTES_RSP_CMD_66 = 6,
                          N_BYTES_RSP_CMD_67 = 5,
                          N_BYTES_RSP_CMD_68 = 42,
                          N_BYTES_RSP_CMD_69 = 1017,
                          N_BYTES_RSP_CMD_70 = 42,
                          N_BYTES_RSP_CMD_71 = 0,
                          N_BYTES_RSP_CMD_72 = 5,
                          N_BYTES_RSP_CMD_73 = 6,
                          N_BYTES_RSP_CMD_74 = 22,
                          N_BYTES_RSP_CMD_75 = 519,
                          N_BYTES_RSP_CMD_76 = 6,
                          N_BYTES_RSP_CMD_77 = 0,
                          N_BYTES_RSP_CMD_78 = 36,
                          N_BYTES_RSP_CMD_79 = 6,
                          N_BYTES_RSP_CMD_80 = 11,
                          N_BYTES_RSP_CMD_81 = 11,
                          N_BYTES_RSP_CMD_82 = 11,
                          N_BYTES_RSP_CMD_83 = 4,
                          N_BYTES_RSP_CMD_84 = 4,
                          N_BYTES_RSP_CMD_85 = 4,
                          N_BYTES_RSP_CMD_86 = 4,
                          N_BYTES_RSP_CMD_87 = 7,
                          N_BYTES_RSP_CMD_88 = 4,
                          N_BYTES_RSP_CMD_89 = 21,
                          N_BYTES_RSP_CMD_90 = 7,
                          N_BYTES_RSP_CMD_91 = 519,
                          N_BYTES_RSP_CMD_92 = 52,
                          N_BYTES_RSP_CMD_93 = 37,
                          N_BYTES_RSP_CMD_94 = 4,
                          N_BYTES_RSP_CMD_95 = SIZE_OF_SETUP + 4,
                          N_BYTES_RSP_CMD_96 = 4,
                          N_BYTES_RSP_CMD_97 = 4,
                          N_BYTES_RSP_CMD_98 = 10,
                          N_BYTES_RSP_CMD_99 = 519,
                          N_BYTES_RSP_CMD_100 = 4,
                          N_BYTES_RSP_CMD_101 = 4,
                          N_BYTES_RSP_CMD_102 = 11,
                          N_BYTES_RSP_CMD_103 = 4,
                          N_BYTES_RSP_CMD_104 = 4,
                          N_BYTES_RSP_CMD_105 = 21,
                          N_BYTES_RSP_CMD_106 = 8,
                          N_BYTES_RSP_CMD_107 = 8,
                          N_BYTES_RSP_CMD_108 = 41, // + TEMPLATE
                          N_BYTES_RSP_CMD_109 = 4,
                          N_BYTES_RSP_CMD_110 = 38,
                          N_BYTES_RSP_CMD_111 = 42,
                          N_BYTES_RSP_CMD_112 = 7,
                          N_BYTES_RSP_CMD_113 = 519,
                          N_BYTES_RSP_CMD_114 = 4,
                          N_BYTES_RSP_CMD_115 = 13,
                          N_BYTES_RSP_CMD_116 = 22,
                          N_BYTES_RSP_CMD_117 = 7,
                          N_BYTES_RSP_CMD_118 = 37,
                          N_BYTES_RSP_CMD_119 = 9, // + FRAME RESPOSTA
                          N_BYTES_RSP_CMD_120 = 9,
                          N_BYTES_RSP_CMD_121 = 17,
                          N_BYTES_RSP_CMD_122 = 5,
                          N_BYTES_RSP_CMD_123 = 38,
                          N_BYTES_RSP_CMD_124 = 519,
                          N_BYTES_RSP_CMD_125 = 519,
                          N_BYTES_RSP_CMD_126 = 519,
                          N_BYTES_RSP_CMD_127 = 34,
                          N_BYTES_RSP_CMD_128 = 21,
                          N_BYTES_RSP_CMD_129 = 13,// + linha
                          N_BYTES_RSP_CMD_130 = 4,
                          N_BYTES_RSP_CMD_131 = 519,
                          N_BYTES_RSP_CMD_132 = 5,
                          N_BYTES_RSP_CMD_133 = 24,
                          N_BYTES_RSP_CMD_134 = 23,
                          N_BYTES_RSP_CMD_135 = 582,
                          N_BYTES_RSP_CMD_136 = 14,
                          N_BYTES_RSP_CMD_137 = 4,
                          N_BYTES_RSP_CMD_138 = 9,
                          N_BYTES_RSP_CMD_139 = 6,
                          N_BYTES_RSP_CMD_140 = 15,
                          N_BYTES_RSP_CMD_141 = 6,

                          N_BYTES_RSP_CMD_142 = 263,
                          N_BYTES_RSP_CMD_143 = 7,
                          N_BYTES_RSP_CMD_144 = 520,
                          N_BYTES_RSP_CMD_145 = 12,
                          N_BYTES_RSP_CMD_146 = 7,
                          N_BYTES_RSP_CMD_147 = 520,
                          N_BYTES_RSP_CMD_148 = 11,
                          N_BYTES_RSP_CMD_149 = 22,
                          N_BYTES_RSP_CMD_150 = 520,
                          N_BYTES_RSP_CMD_151 = 62,
                          N_BYTES_RSP_CMD_152 = 15,
                          N_BYTES_RSP_CMD_153 = 519,
                          N_BYTES_RSP_CMD_154 = 7,
                          N_BYTES_RSP_CMD_155 = 5,
                          N_BYTES_RSP_CMD_156 = 8,
                          N_BYTES_RSP_CMD_157 = 5,
                          N_BYTES_RSP_CMD_158 = 22,
                          N_BYTES_RSP_CMD_159 = 519,
                          N_BYTES_RSP_CMD_160 = 17,
                          N_BYTES_RSP_CMD_161 = 4,
                          N_BYTES_RSP_CMD_162 = 6;


        //---------------------------------------------------------
        // retorno
        //---------------------------------------------------------
        public const int RT_OK = 0,
                          RT_MEMORIA_CHEIA = 1,
                          RT_DISP_JA_APRENDIDO = 2,
                          RT_DISP_NAO_ENCONTRADO = 3,
                          RT_SEM_MAIS_EVENTOS = 4,
                          RT_FIM_DO_ARQUIVO = 5,
                          RT_ESTOURO_BUFFER_INTERNO = 6,
                          RT_CONFLITO_DE_PORTAS = 7,
                          RT_ERRO_NO_TAMANHO_PACOTE = 8,
                          RT_ERRO_DE_CHECKSUM = 9,
                          RT_ERRO_NO_MODO_OPERACAO = 10,
                          RT_ERRO_ESCRITA_LEITURA = 11,
                          RT_ERRO_FORA_DO_LIMITE_DO_DISPOSITIVO = 12,
                          RT_ERRO_TIPO_DIFERENTE = 13,
                          RT_ERRO_OPERACAO_INVALIDA = 14,
                          RT_ERRO_DISPOSITIVO_DIFERENTE = 15,
                          RT_ERRO_INDICE_INVALIDO = 16,
                          RT_ERRO_DE_SINTAXE = 17,
                          RT_ERRO_VALOR_INVALIDO = 18,
                          RT_ERRO_SERIAL_INVALIDO = 19,
                          RT_ERRO_SETOR_DIFERENTE = 20,
                          RT_ERRO_ENDERECO_DIFERENTE = 21,
                          RT_ERRO_BIOMETRIA_1_SUCESSO_BIOMETRIA_2 = 22,
                          RT_ERRO_BIOMETRIA_2_SUCESSO_BIOMETRIA_1 = 23,
                          RT_ERRO_BIOMETRIA_1_E_2 = 24,
                          RT_ERRO_SEM_RESPOSTA_DO_MODULO = 25,
                          RT_ERRO_FECHAR_ARQUIVO = 244,
                          RT_ERRO_MIDIA_LEITURA = 245,
                          RT_ERRO_MIDIA_ESCRITA = 246,
                          RT_ERRO_SEM_ARQUIVO_ABERTO = 247,
                          RT_ERRO_ARQUIVO_ABERTO = 248,
                          RT_ERRO_EOF = 249,
                          RT_ERRO_MIDIA = 250,
                          RT_ERRO_TIMEOUT = 251,
                          RT_ERRO_ARQUIVO = 252,
                          RT_ERRO_PASTA = 253,
                          RT_ERRO_SD = 254,
                          RT_ERRO = 255;

        //---------------------------------------------------------------
        // não habilitado
        public const int
                          VALIDO = 0,
                          INVALIDO_NAO_CADASTRADO = 1,
                          INVALIDO_LEITORA_COFRE = 2,
                          INVALIDO_ANTI_PASSBACK = 3,
                          INVALIDO_SEM_CREDITOS = 4,
                          INVALIDO_DATA_VALIDADE = 5,
                          INVALIDO_TEMPO_ANTICARONA = 6,
                          INVALIDO_LEITORA_NAO_HABILITADA = 7,
                          INVALIDO_FERIADO = 8,
                          INVALIDO_JORNADA_TURNO = 9,
                          INVALIDO_SEM_VAGAS_NIVEL = 10,
                          INVALIDO_REMOTO = 11,
                          INVALIDO_LEITORA_INIBIDA = 12;

        //----------------------------------------------------------------
        //dispositivos
        //----------------------------------------------------------------
        public const int
                          RELÉ_PRINCIPAL = 0,
                          SAÍDA_AUXILIAR_DIGITAL = 1,
                          BUZZER = 2,
                          LEITORA_1 = 3,
                          LEITORA_2 = 4,
                          LEITORA_3 = 5,
                          LEITORA_4 = 6,
                          BIOMETRIA_1 = 7,
                          BIOMETRIA_2 = 8;

        //----------------------------------------------------------------
        // operacao
        //----------------------------------------------------------------
        public const int
                          OP_ESCRITA = 0,
                          OP_EDICAO = 1,
                          OP_RESTORE = 2,
                          OP_LEITURA = 3,
                          OP_APAGAR = 4,
                          OP_ENVIAR_ATUAL_PROXIMO = 5,
                          OP_ENVIAR_NOVAMENTE = 6,
                          OP_DISPOSITIVOS = 7,
                          OP_EVENTOS = 8,
                          OP_BIOMETRIA_NAO_CADASTRADA = 9,
                          OP_APAGAR_TODOS_OS_TEMPLATES = 10,
                          OP_VERIFICAR_ID_GRAVADO = 11,
                          OP_SOLICITAR_ID_VAGO = 12,
                          OP_IDENTIFICAR_TEMPLATE = 13,
                          OP_BIOMETRIA_NAO_CADASTRADA_COM_ID = 14,
                          OP_BIOMETRIA_1_1 = 15,
                          OP_LER_EVENTO_INDEXADO = 16,
                          OP_MARCAR_EVENTO_COMO_LIDO_INDEXADO = 17,
                          OP_LER_ULTIMO_EVENTO_NAO_LIDO = 18,

                          OP_ABRIR_ARQUIVO_LEITURA = 19,
                          OP_LER_ARQUIVO_LEITURA = 20,
                          OP_CRIAR_ABRIR_ARQUIVO_ESCRITA = 21,
                          OP_ESCREVER_ARQUIVO_ESCRITA = 22,
                          OP_FECHAR_ARQUIVO = 23,
                          OP_LISTAR = 24,
                          OP_CRIAR_DIRETORIO = 25,
                          OP_MUDAR_DIRETORIO = 26,
                          OP_REINICIAR_MIDIA = 27,
                          OP_APAGAR_ARQUIVO_DIRETORIO = 28;




        //----------------------------------------------------------------
        // modo remoto
        //----------------------------------------------------------------
        public const int MODO_REMOTO_FIXO_90S = 0,
                         MODO_REMOTO_STAND_BY = 1,
                         MODO_REMOTO_CANCEL_STAND_BY = 2,
                         MODO_REMOTO_TEMPO_CONFIGURAVEL = 3;

        public const int TIPO_BIOMETRIA_SEM_BIOMETRIA = 0,
                         TIPO_BIOMETRIA_MIAXIS = 1,
                         TIPO_BIOMETRIA_VIRDI = 2,
                         TIPO_BIOMETRIA_SUPREMA = 3,
                         TIPO_BIOMETRIA_NITGEN = 4,
                         TIPO_BIOMETRIA_ANVIZ = 5,
                         TIPO_BIOMETRIA_T5S = 6,
                         TIPO_BIOMETRIA_LN3000 = 7;

        public const ushort
            INDICE_CNTATUAL = 0,
            INDICE_ENDCAN = 1,
            INDICE_MODO_OPERACAO = 2,
            INDICE_FLAGSSETUP1 = 3,
            INDICE_FLAGSSETUP2 = 4,
            INDICE_FLAGSSETUP3 = 5,
            INDICE_FLAGSSETUP4 = 6,
            INDICE_FLAGSSETUP5 = 7,
            INDICE_FLAGSSETUP6 = 8,
            INDICE_RESERVADO_9 = 9,
            INDICE_RESERVADO_10 = 10,
            INDICE_NIVEL = 11,
            INDICE_VAGASNIVELH = 12,
            INDICE_TRELE1 = 13,
            INDICE_TRELE2 = 14,
            INDICE_TRELE3 = 15,
            INDICE_TRELE4 = 16,
            INDICE_TSAIDA1 = 17,
            INDICE_TSAIDA2 = 18,
            INDICE_TSAIDA3 = 19,
            INDICE_TSAIDA4 = 20,
            INDICE_TOUTVAL_01 = 21,
            INDICE_TOUTVAL_02 = 22,
            INDICE_TOUTVAL_03 = 23,
            INDICE_TOUTVAL_04 = 24,
            INDICE_TOUTVAL_05 = 25,
            INDICE_TOUTVAL_06 = 26,
            INDICE_TOUTVAL_07 = 27,
            INDICE_TOUTVAL_08 = 28,
            INDICE_TOUTVAL_09 = 29,
            INDICE_TOUTVAL_10 = 30,
            INDICE_CNTATUAL_DISP = 31,
            INDICE_SETOR = 32,
            INDICE_ATRASO_ENTRADA_L1 = 33,
            INDICE_ATRASO_ENTRADA_L2 = 34,
            INDICE_ATRASO_ENTRADA_L3 = 35,
            INDICE_ATRASO_ENTRADA_L4 = 36,
            INDICE_PARAMETRO_01_RS485_L1 = 37,
            INDICE_PARAMETRO_01_RS485_L2 = 38,
            INDICE_PARAMETRO_01_RS485_L3 = 39,
            INDICE_PARAMETRO_01_RS485_L4 = 40,
            INDICE_TOUT_LEITORA_RS485_L1 = 41,
            INDICE_TOUT_LEITORA_RS485_L2 = 42,
            INDICE_TOUT_LEITORA_RS485_L3 = 43,
            INDICE_TOUT_LEITORA_RS485_L4 = 44,
            INDICE_FUNCAO_RS485_L1 = 45,
            INDICE_FUNCAO_RS485_L2 = 46,
            INDICE_FUNCAO_RS485_L3 = 47,
            INDICE_FUNCAO_RS485_L4 = 48,
            INDICE_RESERVADO = 49,
            INDICE_IP = 50,
            INDICE_MASK = 51,
            INDICE_GW = 52,
            INDICE_IPDESTINO = 53,
            INDICE_FLAGS_ETHERNET_1 = 54,
            INDICE_TOUTETHERNET1 = 55,
            INDICE_TOUTETHERNET2 = 56,
            INDICE_PORTA1 = 57,
            INDICE_PORTA2 = 58,
            INDICE_PORTA3 = 59,
            INDICE_PORTA4 = 60,
            INDICE_DDNSUSUARIO = 61,
            INDICE_DDNSSENHA = 62,
            INDICE_DDNSDEVICE = 63,
            INDICE_USUARIOLOGIN = 64,
            INDICE_SENHALOGIN = 65,
            INDICE_DNSHOST = 66,
            INDICE_DDNSHOST = 67,
            INDICE_TOLERANCIA_HORARIO = 68,
            INDICE_FUNCAOLEITORA1 = 69,
            INDICE_FUNCAOLEITORA2 = 70,
            INDICE_FUNCAOLEITORA3 = 71,
            INDICE_FUNCAOLEITORA4 = 72,
            INDICE_TEMPO_LED_BUZZER_LEITORAS = 73,
            INDICE_FLAGSSETUP7 = 74,
            INDICE_TOUTPASSBACK = 75,
            INDICE_FLAGSSETUP8 = 76,
            INDICE_HORARIO_VERAO_INI_DIA = 77,
            INDICE_HORARIO_VERAO_INI_MES = 78,
            INDICE_HORARIO_VERAO_FIM_DIA = 79,
            INDICE_HORARIO_VERAO_FIM_MES = 80,
            INDICE_PORTA5 = 81,
            INDICE_PORTA6 = 82,
            INDICE_FLAGSSETUP9 = 83,
            INDICE_PORTA7 = 84,
            INDICE_TEMPO_ALARME_PORTA_ABERTA = 85,
            INDICE_FLAGSSETUP10 = 86,
            INDICE_FLAGSSETUP11 = 87,
            INDICE_FLAGSSETUP12 = 88,
            INDICE_TOUT_ALARME_PORTA_ABERTA = 89,
            INDICE_FLAGSSETUP13 = 90,
            INDICE_TOUT_ACIONAMENTO_LEDBUZZER = 91,
            INDICE_PORTA_HTTP = 92,
            INDICE_FLAGSSETUP14 = 93,
            INDICE_FLAGSSETUP15 = 94,
            INDICE_TRELE5 = 95,
            INDICE_FLAGSSETUP16 = 96,
            INDICE_PORTA8 = 97,
            INDICE_PORTA9 = 98,
            INDICE_PORTA10 = 99,
            INDICE_PORTA11 = 100,
            INDICE_PORTA12 = 101,
            INDICE_PORTA13 = 102,
            INDICE_FLAGSSETUP17 = 103,
            INDICE_FLAGSSETUP18 = 104,
            INDICE_HORARIOATUALIZAAUTO = 105,
            INDICE_FLAGSSETUP19 = 106,
            INDICE_TEMPOQUEDACARTAO = 107,
            INDICE_TRELE6 = 108,


            INDICE_REBOOT = 109;



        public ushort[] sizeOfparametrosSetup = new ushort[] {
                            1, //INDICE_CNTATUAL = 0,
                            1, //INDICE_ENDCAN = 1,
                            1, //INDICE_MODO_OPERACAO = 2,
                            1, //INDICE_FLAGSSETUP1 = 3,
                            1, //INDICE_FLAGSSETUP2 = 4,
                            1, //INDICE_FLAGSSETUP3 = 5,
                            1, //INDICE_FLAGSSETUP4 = 6,
                            1, //INDICE_FLAGSSETUP5 = 7,
                            1, //INDICE_FLAGSSETUP6 = 8,
                            1, //INDICE_SETOR = 9,
                            1, //INDICE_RESERVADO = 10,
                            1, //INDICE_NIVEL = 11,
                            2, //INDICE_VAGASNIVELH = 12,
                            1, //INDICE_TRELE1 = 14,
                            1, //INDICE_TRELE2 = 15,
                            1, //INDICE_TRELE3 = 16,
                            1, //INDICE_TRELE4 = 17,
                            1, //INDICE_TSAIDA1 = 18,
                            1, //INDICE_TSAIDA2 = 19,
                            1, //INDICE_TSAIDA3 = 20,
                            1, //INDICE_TSAIDA4 = 21,
                            1, //INDICE_TOUTVAL_01 = 22,
                            1, //INDICE_TOUTVAL_02 = 23,
                            1, //INDICE_TOUTVAL_03 = 24,
                            1, //INDICE_TOUTVAL_04 = 25,
                            1, //INDICE_TOUTVAL_05 = 26,
                            1, //INDICE_TOUTVAL_06 = 27,
                            1, //INDICE_TOUTVAL_07 = 28,
                            1, //INDICE_TOUTVAL_08 = 29,
                            1, //INDICE_TOUTVAL_09 = 30,
                            1, //INDICE_TOUTVAL_10 = 31,
                            1, //INDICE_CNTATUAL_DISP = 32,
                            1, //INDICE_SETOR = 33,
                            1, //INDICE_ATRASO_ENTRADA_L1 = 34
                            1, //INDICE_ATRASO_ENTRADA_L2 = 35
                            1, //INDICE_ATRASO_ENTRADA_L3 = 36
                            1, //INDICE_ATRASO_ENTRADA_L4 = 37
                            1, //INDICE_PARAMETRO_01_RS485_L1 = 38,
                            1, //INDICE_PARAMETRO_01_RS485_L2 = 39,
                            1, //INDICE_PARAMETRO_01_RS485_L3 = 40,
                            1, //INDICE_PARAMETRO_01_RS485_L4 = 41,
                            1, //INDICE_TOUT_LEITORA_RS485_L1 = 42,
                            1, //INDICE_TOUT_LEITORA_RS485_L2 = 43,
                            1, //INDICE_TOUT_LEITORA_RS485_L3 = 44,
                            1, //INDICE_TOUT_LEITORA_RS485_L4 = 45,
                            1, //INDICE_FUNCAO_RS485_L1 = 46,
                            1, //INDICE_FUNCAO_RS485_L2 = 47,
                            1, //INDICE_FUNCAO_RS485_L3 = 48,
                            1, //INDICE_FUNCAO_RS485_L4 = 49,
                            SIZE_OF_BYTES_RESERVADOS, //INDICE_RESERVADO = 38,
                            4, //INDICE_IP = 70,
                            4, //INDICE_MASK = 74,
                            4, //INDICE_GW = 78,
                            4, //INDICE_IPDESTINO = 82,
                            1, //INDICE_FLAGS_ETHERNET_1 = 86,
                            1, //INDICE_TOUTETHERNET1 = 87,
                            1, //INDICE_TOUTETHERNET2 = 88,
                            2, //INDICE_PORTA1 = 89,
                            2, //INDICE_PORTA2 = 91,
                            2, //INDICE_PORTA3 = 93,
                            2, //INDICE_PORTA4 = 95,
                            SIZE_OF_DDNS_USUARIO, //INDICE_DDNSUSUARIO = 97,
                            SIZE_OF_DDNS_SENHA, //INDICE_DDNSSENHA = 113,
                            SIZE_OF_DDNS_DEVICE, //INDICE_DDNSDEVICE = 129,
                            SIZE_OF_USUARIO_LOGIN, //INDICE_USUARIOLOGIN = 169,
                            SIZE_OF_SENHA_LOGIN, //INDICE_SENHALOGIN = 185,
                            SIZE_OF_DNS_HOST, //INDICE_DNSHOST = 201,
                            SIZE_OF_DDNS_HOST, //INDICE_DDNSHOST = 217,
                            1, //INDICE_TOLERANCIA_HORARIO = 257,
                            1, //INDICE_FUNCAOLEITORA1 = 258,
                            1, //INDICE_FUNCAOLEITORA2 = 259,
                            1, //INDICE_FUNCAOLEITORA3 = 260,
                            1, //INDICE_FUNCAOLEITORA4 = 261,
                            1, //INDICE_TEMPO_LED_BUZZER_LEITORAS = 262,
                            1, //INDICE_FLAGSSETUP7 = 263,
                            1, //INDICE_TOUTPASSBACK = 264,
                            1, //INDICE_FLAGSSETUP8 = 265,
                            1, //INDICE_HORARIO_VERAO_INI_DIA = 266,
                            1, //INDICE_HORARIO_VERAO_INI_MES = 267,
                            1, //INDICE_HORARIO_VERAO_FIM_DIA = 268,
                            1, //INDICE_HORARIO_VERAO_FIM_MES = 269,
                            2, //INDICE_PORTA5 = 270,
                            2, //INDICE_PORTA6 = 272,
                            1, //INDICE_FLAGSSETUP9 = 274,
                            2, //INDICE_PORTA7 = 275,
                            1, //INDICE_TEMPO_ALARME_PORTA_ABERTA = 277,
                            1, //INDICE_FLAGSSETUP10 = 278,
                            1, //INDICE_FLAGSSETUP11 = 279,
                            1, //INDICE_FLAGSSETUP12 = 280,
                            1, //INDICE_TOUT_ALARME_PORTA_ABERTA = 281,
                            1, //INDICE_FLAGSSETUP13 = 282,
                            1, //INDICE_TOUT_ACIONAMENTO_LEDBUZZER = 283,
                            2, //INDICE_PORTA_HTTP = 284,285
                            1, //INDICE_FLAGSSETUP14 = 286,
                            1, //INDICE_FLAGSSETUP15 = 287,
                            1, //INDICE_TRELE5 = 288,
                            1, //INDICE_FLAGSSETUP16 = 289,
                            2, //INDICE_PORTA8 = 290,
                            2, //INDICE_PORTA9 = 292,
                            2, //INDICE_PORTA10 = 294 295,
                            2, //INDICE_PORTA11 = 296 297,
                            2, //INDICE_PORTA12 = 298 299,
                            2, //INDICE_PORTA13 = 300 301,
                            1, //INDICE_FLAGSSETUP17 = 302,
                            1, //INDICE_FLAGSSETUP18 = 303,
                            2, //INDICE_HORARIOATUALIZAAUTO = 304 305,
                            1, //INDICE_FLAGSSETUP19 = 306
							1, //INDICE_TEMPOQUEDACARTAO = 307,
                            1, //INDICE_TRELE6 = 308,
                            1, //INDICE_REBOOT = 0xAABB
                        };

        public ushort[] parametrosSetup = new ushort[] {
                            /*INDICE_CNTATUAL*/ 0,
                            /*INDICE_ENDCAN*/ 1,
                            /*INDICE_MODO_OPERACAO*/ 2,
                            /*INDICE_FLAGSSETUP1*/ 3,
                            /*INDICE_FLAGSSETUP2*/ 4,
                            /*INDICE_FLAGSSETUP3*/ 5,
                            /*INDICE_FLAGSSETUP4*/ 6,
                            /*INDICE_FLAGSSETUP5*/ 7,
                            /*INDICE_FLAGSSETUP6*/ 8,
                            /*INDICE_RESERVADO_9*/ 9,
                            /*INDICE_RESERVADO_10*/ 10,
                            /*INDICE_NIVEL*/ 11,
                            /*INDICE_VAGASNIVELH*/12,
                            /*INDICE_TRELE1*/14,
                            /*INDICE_TRELE2*/ 15,
                            /*INDICE_TRELE3*/ 16,
                            /*INDICE_TRELE4*/ 17,
                            /*INDICE_TSAIDA1*/ 18,
                            /*INDICE_TSAIDA2*/ 19,
                            /*INDICE_TSAIDA3*/ 20,
                            /*INDICE_TSAIDA4*/ 21,
                            /*INDICE_TOUTVAL_01*/22,
                            /*INDICE_TOUTVAL_02*/ 23,
                            /*INDICE_TOUTVAL_03*/ 24,
                            /*INDICE_TOUTVAL_04*/ 25,
                            /*INDICE_TOUTVAL_05*/ 26,
                            /*INDICE_TOUTVAL_06*/ 27,
                            /*INDICE_TOUTVAL_07*/ 28,
                            /*INDICE_TOUTVAL_08*/ 29,
                            /*INDICE_TOUTVAL_09*/ 30,
                            /*INDICE_TOUTVAL_10*/ 31,
                            /*INDICE_CNTATUAL_DISP*/32,
                            /*INDICE_SETOR*/33,
                            /*INDICE_ATRASO_ENTRADA_L1*/34,
                            /*INDICE_ATRASO_ENTRADA_L2*/35,
                            /*INDICE_ATRASO_ENTRADA_L3*/36,
                            /*INDICE_ATRASO_ENTRADA_L4*/37,
                            /*INDICE_PARAMETRO_01_RS485_L1*/38,
                            /*INDICE_PARAMETRO_01_RS485_L2*/39,
                            /*INDICE_PARAMETRO_01_RS485_L3*/40,
                            /*INDICE_PARAMETRO_01_RS485_L4*/41,
                            /*INDICE_TOUT_LEITORA_RS485_L1*/42,
                            /*INDICE_TOUT_LEITORA_RS485_L2*/43,
                            /*INDICE_TOUT_LEITORA_RS485_L3*/44,
                            /*INDICE_TOUT_LEITORA_RS485_L4*/45,
                            /*INDICE_FUNCAO_RS485_L1*/46,
                            /*INDICE_FUNCAO_RS485_L2*/47,
                            /*INDICE_FUNCAO_RS485_L3*/48,
                            /*INDICE_FUNCAO_RS485_L4*/49,
                            /*INDICE_RESERVADO*/50,
                            /*INDICE_IP*/70,
                            /*INDICE_MASK*/74,
                            /*INDICE_GW*/78,
                            /*INDICE_IPDESTINO*/82,
                            /*INDICE_FLAGS_ETHERNET_1*/86,
                            /*INDICE_TOUTETHERNET1*/ 87,
                            /*INDICE_TOUTETHERNET2*/ 88,
                            /*INDICE_PORTA189*/ 89,
                            /*INDICE_PORTA291*/ 91,
                            /*INDICE_PORTA393*/ 93,
                            /*INDICE_PORTA495*/ 95,
                            /*INDICE_DDNSUSUARIO  */97,
                            /*INDICE_DDNSSENHA  */113,
                            /*INDICE_DDNSDEVICE  */129,
                            /*INDICE_USUARIOLOGIN  */169,
                            /*INDICE_SENHALOGIN  */185,
                            /*INDICE_DNSHOST  */201,
                            /*INDICE_DDNSHOST  */217,
                            /*INDICE_TOLERANCIA_HORARIO  */257,
                            /*INDICE_FUNCAOLEITORA1*/ 258,
                            /*INDICE_FUNCAOLEITORA2*/ 259,
                            /*INDICE_FUNCAOLEITORA3*/ 260,
                            /*INDICE_FUNCAOLEITORA4*/ 261,
                            /*INDICE_TEMPO_LED_BUZZER_LEITORAS*/ 262,
                            /*INDICE_FLAGSSETUP7*/ 263,
                            /*INDICE_TOUTPASSBACK*/ 264,		
                            /*INDICE_FLAGSSETUP8,		  // */265,
                            /*INDICE_HORARIO_VERAO_INI_DIA, // */266,
                            /*INDICE_HORARIO_VERAO_INI_MES, // */267,
                            /*INDICE_HORARIO_VERAO_FIM_DIA, // */268,
                            /*INDICE_HORARIO_VERAO_FIM_MES, // */269,
                            /*INDICE_PORTA5  */270,
                            /*INDICE_PORTA6  */272,
                            /*INDICE_FLAGSSETUP9  */274,
                            /*INDICE_PORTA7  */275,
                            /*INDICE_TEMPO_EVENTO_PORTA_ABERTA  */277,
                            /*INDICE_FLAGSSETUP10, // */278,
                            /*INDICE_FLAGSSETUP11, // */279,
                            /*INDICE_FLAGSSETUP12, //  */280,
                            /*INDICE_TOUT_ALARME_PORTA_ABERTA  */281,
                            /*INDICE_FLAGSSETUP13 */ 282,
                            /*INDICE_TOUT_ACIONAMENTO_LEDBUZZER*/ 283,
                            /*INDICE_PORTA_HTTP*/284, //285
                            /*INDICE_FLAGSSETUP14 */ 286,
                            /*INDICE_FLAGSSETUP15 */ 287,
                            /*INDICE_TRELE5*/288,
                            /*INDICE_FLAGSSETUP16 */ 289,
                            /*INDICE_PORTA8  */290,
                            /*INDICE_PORTA9  */292,
                            /*INDICE_PORTA10  */294,
                            /*INDICE_PORTA11  */296,
                            /*INDICE_PORTA12  */298,
                            /*INDICE_PORTA13  */300,
                            /*INDICE_FLAGSSETUP17 */ 302,
                            /*INDICE_FLAGSSETUP18*/303,
                            /*INDICE_HORARIOATUALIZAAUTO*/304,
                            /*INDICE_FLAGS_SETUP19*/306,
							/*INDICE_TEMPOQUEDACARTAO*/307,
                            /*INDICE_TRELE6*/308,
                            /*INDICE_REBOOT  */0xAABB,
                        };

        public const int
                        FILE_LINEAR = 0,
                        FILE_INDEX = 1, 	//1
                        FILE_DISP = 2, 		//2
                        FILE_SETUP = 3, 	//3
                        FILE_EVENTO = 4, 	//4
                        FILE_ROTAS = 5,		//5
                        FILE_GRUPOS = 6, 	//6
                        FILE_JORNADA = 7, 	//7
                        FILE_TURNOS = 8, 	//8
                        FILE_FERIADOS = 9,  //9
                        FILE_LABELS = 10, 	//10
                        FILE_DISPLAY = 11,  //11
                        FILE_CSV = 12, 		//12
                        FILE_MIAXIS = 13, 	//13
                        FILE_VIRDI = 14, 	//14
                        FILE_NITGEN = 15, 	//15
                        FILE_SUPREMA = 16, 	//16 
                        FILE_ANVIZ = 17, 	//17
                        FILE_DTURNOS = 18; 	//18

        public const byte
                        //---------------------------------------------------------------------------------------
                        // modo de operação
                        //---------------------------------------------------------------------------------------
                        TIPO_CATRACA_TORNIQUETE = 0,
                        TIPO_CADEIRANTE_CANCELA_VERTICAL = 1,
                        TIPO_CADEIRANTE_CANCELA_HORIZONTAL = 2;

        //---------------------------------------------------------------------------------------
        // outras definições
        //---------------------------------------------------------------------------------------
        public const byte
                        MIDIA_SD = 0,
                        MIDIA_USB = 1;

        //--------------------------------------------------------------
        //                          ESTRUTURAS
        //--------------------------------------------------------------

        //--------------------------------------------------------------
        public struct flagsCadastro
        {
            public byte saidaCofre;		//:1 // 0 = saída nas leitora 2 ou 3; 1 = saída somente na leitora 3  (DISP_CA)					
            public byte antiPassback;	//:2 // 0 = não trata antipassback; 1 = entrou; 2 = saiu; 3 = indeterminado
            public byte visitante;      //:1 // 0 = não sera desvinculado ao sair; 1 = será desvincula usuário ao saír (Se placa com setup.f.cfg13.f.desvincularVisitante = 1)
            public byte ctrlVagasId;    //:1 // 0 = sem controle de vagas por id; 1 = com controle de vagas por id
            public byte duplaValidacao; //:1 // 0 = validacao simples / 1 = dupla validacao
            public byte panico;         //:1 // 0 = envia evento de acionamento / envia evento de acionamento + evento de pânico

            public byte val;

            public flagsCadastro(byte _saidaCofre, byte _antiPassback, byte _visitante, byte _ctrlVagasId, byte _duplaValidacao, byte _panico)
            {
                saidaCofre = _saidaCofre;
                antiPassback = _antiPassback;
                visitante = _visitante;
                ctrlVagasId = _ctrlVagasId;
                duplaValidacao = _duplaValidacao;
                panico = _panico;

                val = (byte)((panico << 7) + (duplaValidacao << 6) + (ctrlVagasId << 4) + (visitante << 3) + (antiPassback << 1) + saidaCofre);
            }
        }

        //--------------------------------------------------------------
        public struct flagsStatus
        {
            public byte estadoBateria;			    // 0 = saída nas leitora 2 ou 3; 1 = saída somente na leitora 3  (DISP_CA)					
            public byte ultimoAcionamento;			// 0 = não trata antipassback; 1 = entrou; 2 = saiu; 3 = indeterminado					
            public byte val;

            public flagsStatus(byte _estadoBateria, byte _ultimoAcionamento)
            {
                estadoBateria = _estadoBateria;
                ultimoAcionamento = _ultimoAcionamento;
                val = (byte)((ultimoAcionamento << 4) + estadoBateria);
            }
        }


        //--------------------------------------------------------------
        public struct s_validade
        {
            public byte diaIni, mesIni, anoIni, diaFim, mesFim, anoFim;
            public s_validade(byte _diaIni, byte _mesIni, int _anoIni, byte _diaFim, byte _mesFim, int _anoFim)
            {
                diaIni = _diaIni;
                mesIni = _mesIni;
                if (_anoIni > 1999) { anoIni = (byte)(_anoIni - 2000); } else { anoIni = (byte)_anoIni; }

                diaFim = _diaFim;
                mesFim = _mesFim;
                if (_anoFim > 1999) { anoFim = (byte)(_anoFim - 2000); } else { anoFim = (byte)_anoFim; }
            }
        }

        //------------------------------------------------------------------------------------------------------------------
        // FLAGS_SETUP 1
        public struct FLAGS_CFG_1
        {
            public byte brCan; //:2;
            public byte ctrlVagasNivel; //:1;		
            public byte serialMestre; //:1;
            public byte RFHabilitado; //:1;
            public byte giroCatraca; //:3;		// 0=bidirecional/1=só entrada / 2=só saída / 3=Automático / 4=Automático Leitora / 5=Automático BM

            public FLAGS_CFG_1(byte _brCan, byte _ctrlVagasNivel, byte _serialMestre, byte _RFHabilitado, byte _giroCatraca)
            {
                brCan = _brCan;
                ctrlVagasNivel = _ctrlVagasNivel;
                serialMestre = _serialMestre;
                RFHabilitado = _RFHabilitado;
                giroCatraca = _giroCatraca;
            }
        }

        //------------------------------------------------------------------------------------------------------------------
        // FLAGS_CFG 2
        public struct FLAGS_CFG_2
        {
            public byte sentidoCatraca; //:1;
            public byte umaSolenoide; //:1;
            public byte funcaoUart2; //:3;
            public byte funcaoUart3; //:3;

            public FLAGS_CFG_2(byte _sentidoCatraca, byte _umaSolenoide, byte _funcaoUart2, byte _funcaoUart3)
            {
                sentidoCatraca = _sentidoCatraca;
                umaSolenoide = _umaSolenoide;
                funcaoUart2 = _funcaoUart2;
                funcaoUart3 = _funcaoUart3;
            }
        }

        //------------------------------------------------------------------------------------------------------------------
        // FLAGS_CFG 3
        public struct FLAGS_CFG_3
        {
            public byte baudRateUart1; //:2;	
            public byte paridadeUart1; //:2;	
            public byte stopBitsUart1; //:1;	
            public byte baudRateUart2; //:2;	
            public byte enviarTemplateNaoCadastrado; //:1;	

            public FLAGS_CFG_3(byte _baudRateUart1, byte _paridadeUart1, byte _stopBitsUart1, byte _baudRateUart2, byte _enviarTemplateNaoCadastrado)
            {
                baudRateUart1 = _baudRateUart1;
                paridadeUart1 = _paridadeUart1;
                stopBitsUart1 = _stopBitsUart1;
                baudRateUart2 = _baudRateUart2;
                enviarTemplateNaoCadastrado = _enviarTemplateNaoCadastrado;
            }

        }

        //------------------------------------------------------------------------------------------------------------------
        // FLAGS_CFG 4
        public struct FLAGS_CFG_4
        {
            public byte baudRateUart3; //:2;
            public byte tipoDispLeitora1; //:3;
            public byte tipoDispLeitora2; //:3;

            public FLAGS_CFG_4(byte _baudRateUart3, byte _tipoDispLeitora1, byte _tipoDispLeitora2)
            {
                baudRateUart3 = _baudRateUart3;
                tipoDispLeitora1 = _tipoDispLeitora1;
                tipoDispLeitora2 = _tipoDispLeitora2;
            }

        }

        //------------------------------------------------------------------------------------------------------------------
        // FLAGS_CFG 5
        public struct FLAGS_CFG_5
        {
            public byte tipoDispLeitora3; //:3;	
            public byte tipoDispLeitora4; //:3;	
            public byte sinalizacaoSaidasDigitais; //:2;	

            public FLAGS_CFG_5(byte _tipoDispLeitora3, byte _tipoDispLeitora4, byte _sinalizacaoSaidasDigitais)
            {
                tipoDispLeitora3 = _tipoDispLeitora3;
                tipoDispLeitora4 = _tipoDispLeitora4;
                sinalizacaoSaidasDigitais = _sinalizacaoSaidasDigitais;
            }
        }

        //------------------------------------------------------------------------------------------------------------------
        // FLAGS_CFG 6
        public struct FLAGS_CFG_6
        {
            public byte eventoSensor1; //:2;  // 0 = borda de subida / 1 = borda descida / 2 = passagem com sensor2			
            public byte eventoSensor2; //:2;  // 0 = borda de subida / 1 = borda descida 			
            public byte eventoSensor3; //:2;  // 0 = borda de subida / 1 = borda descida / 2 = passagem com sensor4			
            public byte eventoSensor4; //:2;  // 0 = borda de subida / 1 = borda descida 			

            public FLAGS_CFG_6(byte _eventoSensor1, byte _eventoSensor2, byte _eventoSensor3, byte _eventoSensor4)
            {
                eventoSensor1 = _eventoSensor1;
                eventoSensor2 = _eventoSensor2;
                eventoSensor3 = _eventoSensor3;
                eventoSensor4 = _eventoSensor4;
            }
        }

        //------------------------------------------------------------------------------------------------------------------
        // FLAGS_CFG 7
        public struct FLAGS_CFG_7
        {
            public byte toutPanico; //:3;   // 0 = imediato/ 1 a 5 = 1 a 5 segundos / 6 = desligado
            public byte avisoLowBat; //:1;  // 0 = não gera/envia evento de bateria baixa/ 1 = gera/envia evento de bateria baixa	
            public byte toutMsgDisplay; //:4; // ( 0 a 15 ) = 5 a 20 s

            public FLAGS_CFG_7(byte _toutPanico, byte _avisoLowBat, byte _toutMsgDisplay)
            {
                toutPanico = _toutPanico;
                avisoLowBat = _avisoLowBat;
                toutMsgDisplay = _toutMsgDisplay;
            }
        }

        //------------------------------------------------------------------------------------------------------------------
        // FLAGS_CFG 8
        public struct FLAGS_CFG_8
        {
            public byte catraca2010; //:2;   		// 0 = catraca normal / 1 = catraca 2010 / 2 = catraca 2010 CT
            public byte saidaLivre; //:1;   		// 0 = saida controlada; 1 = saida livre
            public byte leitoraManchester; //:1;  // 0 = wiegand; 1 = manchester
            public byte autonomia; //:2;   		// 0 = Local / 1 = Remoto / 2 = Remoto temporizado / 3 = mestre
            public byte horarioVeraoHabilitado; //:1;		// 0 = não habilitado / 1 = habilitado (ajuste automático do horário de verão)							
            public byte emHorarioVerao; //:1;		// 0 = fora do horário de verão / 1 = dentro do horário de verão ( relógio já adiantado em uma hora )							

            public FLAGS_CFG_8(byte _catraca2010, byte _saidaLivre, byte _leitoraManchester, byte _autonomia, byte _horarioVeraoHabilitado, byte _emHorarioVerao)
            {
                catraca2010 = _catraca2010;
                saidaLivre = _saidaLivre;
                leitoraManchester = _leitoraManchester;
                autonomia = _autonomia;
                horarioVeraoHabilitado = _horarioVeraoHabilitado;
                emHorarioVerao = _emHorarioVerao;
            }

        }

        //------------------------------------------------------------------------------------------------------------------
        // FLAGS_CFG 9
        public struct FLAGS_CFG_9
        {
            public byte catracaAdaptada; //:1;	    // 0 = catraca normal / 1 = catraca adaptada para enviar pulso de habilitação e receber pulso de evento de passagem	
            public byte tratarBiometria_1_1; //:1;	// 0 = tratamento de biometria 1:N / 1 = envio de evento de biometria 1:1 ( biometria + dispositivo associado )
            public byte antipassbackDesligado; //:1 // 0 = trata anti-passback de acordo com cadastro / 1 = não trata anti-passback
            public byte umSensorParaDuasLeitoras_1e2; //:1; 	// 0 = evento do sensor 1/2 definido por cada sensor / evento do sensor 1/2 definido pelo estado do anti-passback;
            public byte considerarWiegand26bits; //:1 // 0 = não trunca leitura wiegand acima de 26 bits / 1 = trunca leitura wiegand em 26 bits
            public byte alarmeEntradaDigitalAberta; //:1 // 0 = alarme de entrada digital aberta desligado / 1 = alarme de entrada digital ligado
            public byte eventoEntradaDigitalAberta; //:1 // 0 = não envia evento de porta aberta / 1 = envia evento de porta aberta
            public byte umSensorParaDuasLeitoras_3e4; //:1; 	// 0 = evento do sensor 3/4 definido por cada sensor / evento do sensor 3/4 definido pelo estado do anti-passback;

            public FLAGS_CFG_9(byte _catracaAdaptada, byte _tratarBiometria_1_1, byte _antipassbackDesligado, byte _umSensorParaDuasLeitoras_1e2, byte _considerarWiegand26bits, byte _alarmeEntradaDigitalAberta, byte _eventoEntradaDigitalAberta, byte _umSensorParaDuasLeitoras_3e4)
            {
                catracaAdaptada = _catracaAdaptada;
                tratarBiometria_1_1 = _tratarBiometria_1_1;
                antipassbackDesligado = _antipassbackDesligado;
                umSensorParaDuasLeitoras_1e2 = _umSensorParaDuasLeitoras_1e2;
                considerarWiegand26bits = _considerarWiegand26bits;
                alarmeEntradaDigitalAberta = _alarmeEntradaDigitalAberta;
                eventoEntradaDigitalAberta = _eventoEntradaDigitalAberta;
                umSensorParaDuasLeitoras_3e4 = _umSensorParaDuasLeitoras_3e4;
            }
        }

        //------------------------------------------------------------------------------------------------------------------
        // FLAGS_CFG 10
        public struct FLAGS_CFG_10
        {
            public byte filtrarEvento0; //:1;       // 0 = enviar e gravar na memória o evento 0 / 1 = não enviar nem gravar na memória o evento 0
            public byte filtrarEvento1; //:1;       // 0 = enviar e gravar na memória o evento 1 / 1 = não enviar nem gravar na memória o evento 1
            public byte filtrarEvento2; //:1;       // 0 = enviar e gravar na memória o evento 2 / 1 = não enviar nem gravar na memória o evento 2
            public byte filtrarEvento3; //:1;       // 0 = enviar e gravar na memória o evento 3 / 1 = não enviar nem gravar na memória o evento 3
            public byte filtrarEvento4; //:1;       // 0 = enviar e gravar na memória o evento 4 / 1 = não enviar nem gravar na memória o evento 4
            public byte filtrarEvento5; //:1;       // 0 = enviar e gravar na memória o evento 5 / 1 = não enviar nem gravar na memória o evento 5
            public byte filtrarEvento6; //:1;       // 0 = enviar e gravar na memória o evento 6 / 1 = não enviar nem gravar na memória o evento 6
            public byte filtrarEvento7; //:1;       // 0 = enviar e gravar na memória o evento 7 / 1 = não enviar nem gravar na memória o evento 7

            public FLAGS_CFG_10(byte _filtrarEvento0,
                                    byte _filtrarEvento1,
                                    byte _filtrarEvento2,
                                    byte _filtrarEvento3,
                                    byte _filtrarEvento4,
                                    byte _filtrarEvento5,
                                    byte _filtrarEvento6,
                                    byte _filtrarEvento7)
            {
                filtrarEvento0 = _filtrarEvento0;
                filtrarEvento1 = _filtrarEvento1;
                filtrarEvento2 = _filtrarEvento2;
                filtrarEvento3 = _filtrarEvento3;
                filtrarEvento4 = _filtrarEvento4;
                filtrarEvento5 = _filtrarEvento5;
                filtrarEvento6 = _filtrarEvento6;
                filtrarEvento7 = _filtrarEvento7;
            }
        }
        //------------------------------------------------------------------------------------------------------------------
        // FLAGS_CFG 11
        public struct FLAGS_CFG_11
        {
            public byte filtrarEvento8; //:1;       // 0 = enviar e gravar na memória o evento 8 / 1 = não enviar nem gravar na memória o evento 8
            public byte filtrarEvento9; //:1;       // 0 = enviar e gravar na memória o evento 9 / 1 = não enviar nem gravar na memória o evento 9
            public byte filtrarEvento10; //:1;       // 0 = enviar e gravar na memória o evento 10 / 1 = não enviar nem gravar na memória o evento 10
            public byte filtrarEvento11; //:1;       // 0 = enviar e gravar na memória o evento 11 / 1 = não enviar nem gravar na memória o evento 11
            public byte filtrarEvento12; //:1;       // 0 = enviar e gravar na memória o evento 12 / 1 = não enviar nem gravar na memória o evento 12
            public byte filtrarEvento13; //:1;       // 0 = enviar e gravar na memória o evento 13 / 1 = não enviar nem gravar na memória o evento 13
            public byte filtrarEvento14; //:1;       // 0 = enviar e gravar na memória o evento 14 / 1 = não enviar nem gravar na memória o evento 14
            public byte filtrarEvento15; //:1;       // 0 = enviar e gravar na memória o evento 15 / 1 = não enviar nem gravar na memória o evento 15

            public FLAGS_CFG_11(byte _filtrarEvento8,
                                    byte _filtrarEvento9,
                                    byte _filtrarEvento10,
                                    byte _filtrarEvento11,
                                    byte _filtrarEvento12,
                                    byte _filtrarEvento13,
                                    byte _filtrarEvento14,
                                    byte _filtrarEvento15)
            {
                filtrarEvento8 = _filtrarEvento8;
                filtrarEvento9 = _filtrarEvento9;
                filtrarEvento10 = _filtrarEvento10;
                filtrarEvento11 = _filtrarEvento11;
                filtrarEvento12 = _filtrarEvento12;
                filtrarEvento13 = _filtrarEvento13;
                filtrarEvento14 = _filtrarEvento14;
                filtrarEvento15 = _filtrarEvento15;
            }
        }
        //------------------------------------------------------------------------------------------------------------------
        // FLAGS_CFG 12
        public struct FLAGS_CFG_12
        {
            public byte filtrarEvento16; //:1;       // 0 = enviar e gravar na memória o evento 16 / 1 = não enviar nem gravar na memória o evento 16
            public byte filtrarEvento17; //:1;       // 0 = enviar e gravar na memória o evento 17 / 1 = não enviar nem gravar na memória o evento 17
            public byte filtrarEvento18; //:1;       // 0 = enviar e gravar na memória o evento 18 / 1 = não enviar nem gravar na memória o evento 18
            public byte filtrarEvento19; //:1;       // 0 = enviar e gravar na memória o evento 19 / 1 = não enviar nem gravar na memória o evento 19
            public byte filtrarEvento20; //:1;       // 0 = enviar e gravar na memória o evento 20 / 1 = não enviar nem gravar na memória o evento 20
            public byte filtrarEvento21; //:1;       // 0 = enviar e gravar na memória o evento 21 / 1 = não enviar nem gravar na memória o evento 21
            public byte filtrarEvento22; //:1;       // 0 = enviar e gravar na memória o evento 22 / 1 = não enviar nem gravar na memória o evento 22
            public byte filtrarEvento23; //:1;       // 0 = enviar e gravar na memória o evento 23 / 1 = não enviar nem gravar na memória o evento 23

            public FLAGS_CFG_12(byte _filtrarEvento16,
                                    byte _filtrarEvento17,
                                    byte _filtrarEvento18,
                                    byte _filtrarEvento19,
                                    byte _filtrarEvento20,
                                    byte _filtrarEvento21,
                                    byte _filtrarEvento22,
                                    byte _filtrarEvento23)
            {
                filtrarEvento16 = _filtrarEvento16;
                filtrarEvento17 = _filtrarEvento17;
                filtrarEvento18 = _filtrarEvento18;
                filtrarEvento19 = _filtrarEvento19;
                filtrarEvento20 = _filtrarEvento20;
                filtrarEvento21 = _filtrarEvento21;
                filtrarEvento22 = _filtrarEvento22;
                filtrarEvento23 = _filtrarEvento23;
            }
        }
        //------------------------------------------------------------------------------------------------------------------
        // FLAGS_CFG 13
        public struct FLAGS_CFG_13
        {
            public byte nivelSegurancaBiometria; //:3;      // 0 a 7 ( 0 = mínimo; 7=máximo )
            public byte desvincularVisitante; //:1;         //  0 = não desvincula visitantes na saída / 1 = desvincula visitantes na saída (desvincular == gravar 0 nos créditos)
            public byte enviarEventoIndexado; //:1;         // 0 = envia evento automático sem indice (CMD=116); 1 = envia evento automático com indice (CMD=134)
            public byte validarBiometriaOnline_emRemoto;    //:1;	// 0 = tratar biometria em remoto / 1 = não tratar biometria em remoto (enviar como não cadastrado - validação online)
            public byte controleVagasRota; //:1;		    // 0 =  controle de vagas por código de rota desabilitado / 1 = controla vagas por código de rota habilitado
            public byte bimetria2panico; // :1              // 0 =  envia evento de acinaonamento para biometria 2 de dispositivos ANVIZ-LINEAR / 1 = envia evento de pânico para biometria 2 de dispositivos ANVIZ-LINEAR

            public FLAGS_CFG_13(byte _nivelSegurancaBiometria, byte _desvincularVisitante, byte _enviarEventoIndexado, byte _validarBiometriaOnline_emRemoto, byte _controleVagasRota, byte _bimetria2panico)
            {
                nivelSegurancaBiometria = _nivelSegurancaBiometria;
                desvincularVisitante = _desvincularVisitante;
                enviarEventoIndexado = _enviarEventoIndexado;
                validarBiometriaOnline_emRemoto = _validarBiometriaOnline_emRemoto;
                controleVagasRota = _controleVagasRota;
                bimetria2panico = _bimetria2panico;
            }
        }
        //------------------------------------------------------------------------------------------------------------------
        // FLAGS_CFG 14
        public struct FLAGS_CFG_14
        {
            public byte panicoSaidaDigital; //:2;      // 0 = não aciona saída digital com evento de pânico / 1..7 = sinal de pânico na saída digital 
                                            // Obs.: 1 = pulso da saída digital; 2 = intermitente 60s; 3 = intermitente até receber comando 151 = cancela sinalização de pânico; 
            public byte cartao2xPanico; //:1           // 0 = não / 1 = gera evento de pânico se aproximar o cartão 2 vezes dentro de um intervalo de 3 segundos; 
            public byte tipoTag_UHF; //:2;						// 1 = EPC/ 2 = TID / 0 = LINEAR / 3 = VAGO
            public byte varreSimultanea_UHF; //:1;				// 0 = varredura por pooling/ 1 = varredura simultanea
            public byte wiegand34_UHF; //:1;					// 0 = wiegand 26 bits / 1 = wiegand 34 bits
            public byte frequenciaHopping_UHF; //:1;				// 0 = frequencia uhf fixa / 1 = frequencia uhf hopping


            public FLAGS_CFG_14(byte _panicoSaidaDigital, byte _cartao2xPanico, byte _tipoTag_UHF, byte _varreSimultanea_UHF, byte _wiegand34_UHF, byte _frequenciaHopping_UHF)
            {
                panicoSaidaDigital = _panicoSaidaDigital;
                cartao2xPanico = _cartao2xPanico;
                tipoTag_UHF = _tipoTag_UHF;
                varreSimultanea_UHF = _varreSimultanea_UHF;
                wiegand34_UHF = _wiegand34_UHF;
                frequenciaHopping_UHF = _frequenciaHopping_UHF;
            }
        }
        //------------------------------------------------------------------------------------------------------------------
        // FLAGS_CFG 15
        public struct FLAGS_CFG_15
        {
            public byte buzzer_L1_UHF_on; //:1;  // 0 = buzzer da leitora desligado / 1 = buzzer da leitora ligado
            public byte buzzer_L2_UHF_on; //:1; // 0 = buzzer da leitora desligado / 1 = buzzer da leitora ligado
            public byte buzzer_L3_UHF_on; //:1; // 0 = buzzer da leitora desligado / 1 = buzzer da leitora ligado
            public byte buzzer_L4_UHF_on; //:1; // 0 = buzzer da leitora desligado / 1 = buzzer da leitora ligado
            public byte rele5_eventoClonagem;  //:1;	// 0 = não aciona rele 5 no caso de clonagem / aciona rele 5 no caso de clonagem
            public byte rele5_eventoPanico;  //:1;		// 0 = não aciona rele 5 no caso de panico / aciona rele 5 no caso de panico
            public byte rele5_eventoAlarme; //:1;		// 0 = não aciona rele 5 no caso de alarme / aciona rele 5 no caso de alarme
            public byte rele5_eventoLigado; // :1;      // 0 = não aciona rele 5 ao ligar / aciona rele 5 ao ligar

            public FLAGS_CFG_15(byte _buzzer_L1_UHF_on, byte _buzzer_L2_UHF_on, byte _buzzer_L3_UHF_on, byte _buzzer_L4_UHF_on,
                                byte _rele5_eventoClonagem, byte _rele5_eventoPanico, byte _rele5_eventoAlarme, byte _rele5_eventoLigado)
            {
                buzzer_L1_UHF_on = _buzzer_L1_UHF_on;
                buzzer_L2_UHF_on = _buzzer_L2_UHF_on;
                buzzer_L3_UHF_on = _buzzer_L3_UHF_on;
                buzzer_L4_UHF_on = _buzzer_L4_UHF_on;
                rele5_eventoClonagem = _rele5_eventoClonagem;
                rele5_eventoPanico = _rele5_eventoPanico;
                rele5_eventoAlarme = _rele5_eventoAlarme;
                rele5_eventoLigado = _rele5_eventoLigado;

            }
        }
        //------------------------------------------------------------------------------------------------------------------
        // FLAGS_CFG 16
        public struct FLAGS_CFG_16
        {
            public byte senha13digitos; //:1 -  0 = senha 6 digitos / 1 = senha 13 digitos (7 unidade + 6 senha)
            public byte tipoCatraca;    //:4 - 0000(0) = torniquete / 
                                        // 0001(1) = catraca deficiente 01 (igual a flag9.catracaAdaptada)
                                        // 0010(2) = catraca deficiente 02
                                        // 0011(3) a 1111(15) = vago
            public byte direcaoTurnos;  //:1 - 0 = sem controle de direção para turnos / 1 = controle de direção para turnos habilitado
            public byte panicoSenha99;  //:1 - 0 = não considera senha final 99 como pânico / 1 = considera senha final 99 como pânico
            public byte desligarBuzzer; //:1 - 0 = buzzer ligado / 1 = buzzer desligado

            public FLAGS_CFG_16(byte _senha13digitos, byte _tipoCatraca, byte _direcaoTurnos, byte _panicoSenha99, byte _desligarBuzzer)
            {
                senha13digitos = _senha13digitos;
                tipoCatraca = _tipoCatraca;
                direcaoTurnos = _direcaoTurnos;
                panicoSenha99 = _panicoSenha99;
                desligarBuzzer = _desligarBuzzer;
            }
        }
        //------------------------------------------------------------------------------------------------------------------
        // FLAGS_CFG 17
        public struct FLAGS_CFG_17
        {
            public byte baudrateRS485_1; //:2 bits;  // 0 = 2400/ 1 = 9600 / 2 = 19200 / 3 = 57600
            public byte baudrateRS485_2; //:2 bits;  // 0 = 2400/ 1 = 9600 / 2 = 19200 / 3 = 57600
            public byte baudrateRS485_3; //:2 bits;  // 0 = 2400/ 1 = 9600 / 2 = 19200 / 3 = 57600
            public byte baudrateRS485_4; //:2 bits;  // 0 = 2400/ 1 = 9600 / 2 = 19200 / 3 = 57600

            public FLAGS_CFG_17(byte _baudrateRS485_1, byte _baudrateRS485_2, byte _baudrateRS485_3, byte _baudrateRS485_4)
            {
                baudrateRS485_1 = _baudrateRS485_1;
                baudrateRS485_2 = _baudrateRS485_2;
                baudrateRS485_3 = _baudrateRS485_3;
                baudrateRS485_4 = _baudrateRS485_4;
            }
        }
        //------------------------------------------------------------------------------------------------------------------
        // FLAGS_CFG 18
        public struct FLAGS_CFG_18
        {
            public byte retardoDesligarSolenoide;           //:2 bits;   // 0 = sem tempo de retardo / 1 = retarndo de 0,5 / 2 = retardo de 1s / 3 = retardo de 1,5s
            public byte desligaRetornoAutoT5S;              //:1 bits;   // 0 = Retorno Automatico desativado / 1 = Ativado
            public byte facilityWieg66;                     //:1 bits;   // 0 = Filtra a saida do evento de acordo com a entrada Wiegand 66/ 1 = Saida será sempre o facility code + UserID
            public byte habilitaContagemPassagem;           //:1 bits;   // 0 = Desabilita a contagem de passagens usando creditos e vagas de preset / 1 = habilita a contagem de creditos
            public byte habilitaAtualizacaoAuto;            //:1 bits;   // 0 = Desabilita a atualização de creditos em um horario especifico / 1 = habilita a atualização automatica de creditos usando o preset de vagas da rota.
            public byte habilitaIDMestre;				    //:1 bits;   //0 = Desabilitado / 1 = Habilita modo “mestre” para Dispositivos com Identificação “*” no ultimo caracter da Identificação (byte 14)
            public byte habilitaAcomodacao;  			    // 0 = Desativado / 1 = Habilita tempo de acomodação do sensor (de acordo com atrasoEntradaLeitoraX).


            public FLAGS_CFG_18(byte _retardoDesligarSolenoide, byte _desligaRetornoAutoT5S, byte _facilityWieg66, byte _habilitaContagemPassagem, byte _habilitaAtualizacaoAuto, byte _habilitaIDMestre, byte _habilitaAcomodacao)
            {
                retardoDesligarSolenoide = _retardoDesligarSolenoide;
                desligaRetornoAutoT5S = _desligaRetornoAutoT5S;
                facilityWieg66 = _facilityWieg66;
                habilitaContagemPassagem = _habilitaContagemPassagem;
                habilitaAtualizacaoAuto = _habilitaAtualizacaoAuto;
                habilitaIDMestre = _habilitaIDMestre;
                habilitaAcomodacao = _habilitaAcomodacao;
            }
        }
        //------------------------------------------------------------------------------------------------------------------
        // FLAGS_CFG 19
        public struct FLAGS_CFG_19
        {
            public byte sensorPassagemCofre;  		   // 0 = Desativado; 1 = habilita a utilização do sensor auxiliar 3 para filtrar a passagem do cartão no cofre coletor.
            public byte sw_watchdog;                   // 0 = Desativado; 1 = Habilita o  WatchDog para 30 segundos. 
            public byte verificarFirmwareFTP;          // 0 = Desativado; 1 = Habilita verificação automática de firmware atual.
            public byte sensorGiroInvertido;           // 0 = Repouso sensor = GND ; 1 = Repouso sensor = VDD
            public byte desativaRelePassagem;          // 0 = Desabilitado; 1 = habilita o desligamento do rele após um evento de passagem. 
            public byte antipassbackDesligado_entrada; // 0 = Antipassback valido para entrada e saída; 1 = Antipassback valido somente para saida;
            public byte antipassbackDesligado_saida;   // 0 = Antipassback valido para entrada e saída; 1 = Antipassback valido somente para entrada;
            public byte duplaValidacaoVisitante;


            public FLAGS_CFG_19(byte _sensorPassagemCofre, byte _sw_watchdog, byte _sensorGiroInvertido, byte _desativaRelePassagem, byte _verificarFirmwareFTP, byte _antipassbackDesligado_entrada, byte _antipassbackDesligado_saida, byte _duplaValidacaoVisitante)
            {
                sensorPassagemCofre = _sensorPassagemCofre;
                sw_watchdog = _sw_watchdog;
                verificarFirmwareFTP = _verificarFirmwareFTP;
                sensorGiroInvertido = _sensorGiroInvertido;
                desativaRelePassagem = _desativaRelePassagem;
                antipassbackDesligado_entrada = _antipassbackDesligado_entrada;
                antipassbackDesligado_saida = _antipassbackDesligado_saida;
                duplaValidacaoVisitante = _duplaValidacaoVisitante;
            }
        }
        //------------------------------------------------------------------------------------------------------------------
        // FLAGS_CFG E
        public struct FLAGS_CFG_E
        {
            public byte DDNS; //:2;
            public byte dhcp; //:1;
            public byte DDNShabilitado; //:1;

            public FLAGS_CFG_E(byte _DDNS, byte _dhcp, byte _DDNShabilitado)
            {
                DDNS = _DDNS;
                dhcp = _dhcp;
                DDNShabilitado = _DDNShabilitado;
            }
        }

        //------------------------------------------------------------------------------------------------------------------
        public struct s_setup
        {
            public byte cntAtual;
            public byte endCan;
            public byte modo;
            public FLAGS_CFG_1 cfg1;
            public FLAGS_CFG_2 cfg2;
            public FLAGS_CFG_3 cfg3;
            public FLAGS_CFG_4 cfg4;
            public FLAGS_CFG_5 cfg5;
            public FLAGS_CFG_6 cfg6;
            public byte reservado_9;
            public byte reservado_10;
            public byte nivel;
            public UInt16 vagas;
            public byte[] tRele;
            public byte[] tSaida;
            public byte tempoPassagem;
            public byte tempoLeituraCofre;
            public byte[] toutPassagem;
            public byte[] anticarona;
            public byte cntAtualDisp;
            public byte setor;
            public byte[] atrasoEntradaLeitora;
            public byte[] parametro_01_Leitora;
            public byte[] tLeitoraRS485_10ms;
            public byte[] funcaoRS485;
            public byte[] reservado;
            public UInt32 ip;
            public UInt32 mask;
            public UInt32 gw;
            public UInt32 ipDestino;
            public FLAGS_CFG_E cfge;
            public byte toutEthernet1;
            public byte toutEthernet2;
            public UInt16 porta1;
            public UInt16 porta2;
            public UInt16 porta3;
            public UInt16 porta4;
            public byte[] DDNSusuario;
            public byte[] DDNSsenha;
            public byte[] DDNSdevice;
            public byte[] usuarioLogin;
            public byte[] senhaLogin;
            public byte[] dnsHost;
            public byte[] DDNShost;
            public byte toleranciaHorario;
            public byte[] funcaoLeitora;
            public byte tempoLedBuzzerLeitoras;
            public FLAGS_CFG_7 cfg7;
            public byte toutPassback;
            public FLAGS_CFG_8 cfg8;
            public byte[] horarioVerao;
            public UInt16 porta5; // UART 2
            public UInt16 porta6; // UART 3
            public FLAGS_CFG_9 cfg9;
            public UInt16 porta7;
            public byte tempoEventoEntradaDigitalAberta;
            public FLAGS_CFG_10 cfg10;
            public FLAGS_CFG_11 cfg11;
            public FLAGS_CFG_12 cfg12;
            public byte toutAlarmeEntradaDigitalAberta;
            public FLAGS_CFG_13 cfg13;
            public byte toutAcionamentoLedBuzzer;
            public UInt16 portaHttp;
            public FLAGS_CFG_14 cfg14;
            public FLAGS_CFG_15 cfg15;
            public byte tRele5;
            public FLAGS_CFG_16 cfg16;
            public UInt16 porta8;
            public UInt16 porta9;
            public UInt16 porta10;
            public UInt16 porta11;
            public UInt16 porta12;
            public UInt16 porta13;
            public FLAGS_CFG_17 cfg17;
            public FLAGS_CFG_18 cfg18;
            public byte[] horarioAtualizaAuto;
            public FLAGS_CFG_19 cfg19;
            public byte TempoQuedaCartao;
            public byte tRele6;

            public s_setup(byte _cntAtual, byte _endCan, byte _modo, byte _reservado_9, byte _reservado_10, byte _nivel, UInt16 _vagas, byte[] _tRele, byte[] _tSaida, byte _tempoPassagem,
                             byte _tempoLeituraCofre, byte[] _toutPassagem, byte[] _anticarona, byte _cntAtualDisp, byte _setor, byte[] _atrasoEntradaLeitora, byte[] _parametro_01_Leitora, byte[] _tLeitoraRS485_10ms, byte[] _funcaoRS485, byte[] _reservado,
                            UInt32 _ip, UInt32 _mask, UInt32 _gw, UInt32 _ipDestino, byte _toutEthernet1, byte _toutEthernet2, UInt16 _porta1, UInt16 _porta2, UInt16 _porta3, UInt16 _porta4, byte[] _DDNSusuario, byte[] _DDNSsenha, byte[] _DDNSdevice,
                             byte[] _usuarioLogin, byte[] _senhaLogin, byte[] _dnsHost, byte[] _DDNShost, byte _toleranciaHorario, byte[] _funcaoLeitora, byte _tempoLedBuzzerLeitoras,
                             byte _toutPassback, byte[] _horarioVerao, UInt16 _porta5, UInt16 _porta6, UInt16 _porta7, UInt16 _porta8, UInt16 _porta9, UInt16 _porta10, UInt16 _porta11, UInt16 _porta12, UInt16 _porta13, byte _tempoEventoEntradaDigitalAberta, byte _toutAlarmeEntradaDigitalAberta, byte _toutAcionamentoLedBuzzer, UInt16 _portaHttp, byte _tRele5, byte _tRele6,
                             FLAGS_CFG_E _cfge, FLAGS_CFG_1 _cfg1, FLAGS_CFG_2 _cfg2, FLAGS_CFG_3 _cfg3, FLAGS_CFG_4 _cfg4, FLAGS_CFG_5 _cfg5, FLAGS_CFG_6 _cfg6, FLAGS_CFG_7 _cfg7, FLAGS_CFG_8 _cfg8, FLAGS_CFG_9 _cfg9, FLAGS_CFG_10 _cfg10, FLAGS_CFG_11 _cfg11, FLAGS_CFG_12 _cfg12, FLAGS_CFG_13 _cfg13, FLAGS_CFG_14 _cfg14, FLAGS_CFG_15 _cfg15, FLAGS_CFG_16 _cfg16, FLAGS_CFG_17 _cfg17, FLAGS_CFG_18 _cfg18, byte[] _horarioAtualizaAuto, FLAGS_CFG_19 _cfg19, byte _TempoQuedaCartao)
            {
                cfg1 = _cfg1;
                cfg2 = _cfg2;
                cfg3 = _cfg3;
                cfg4 = _cfg4;
                cfg5 = _cfg5;
                cfg6 = _cfg6;
                cfg7 = _cfg7;
                cfg8 = _cfg8;
                cfg9 = _cfg9;
                cfg10 = _cfg10;
                cfg11 = _cfg11;
                cfg12 = _cfg12;
                cfg13 = _cfg13;
                cfg14 = _cfg14;
                cfg15 = _cfg15;
                cfg16 = _cfg16;
                cfg17 = _cfg17;
                cfg18 = _cfg18;
                cfg19 = _cfg19;
                cfge = _cfge;

                cntAtual = _cntAtual;
                endCan = _endCan;
                modo = _modo;
                reservado_9 = _reservado_9;
                reservado_10 = _reservado_10;
                nivel = _nivel;
                vagas = _vagas;
                tRele = _tRele;
                tSaida = _tSaida;
                tempoPassagem = _tempoPassagem;
                tempoLeituraCofre = _tempoLeituraCofre;
                toutPassagem = _toutPassagem;
                anticarona = _anticarona;
                cntAtualDisp = _cntAtualDisp;
                atrasoEntradaLeitora = _atrasoEntradaLeitora;
                setor = _setor;
                reservado = _reservado;
                ip = _ip;
                mask = _mask;
                gw = _gw;
                ipDestino = _ipDestino;
                toutEthernet1 = _toutEthernet1;
                toutEthernet2 = _toutEthernet2;
                porta1 = _porta1;
                porta2 = _porta2;
                porta3 = _porta3;
                porta4 = _porta4;
                DDNSusuario = _DDNSusuario;
                DDNSsenha = _DDNSsenha;
                DDNSdevice = _DDNSdevice;
                usuarioLogin = _usuarioLogin;
                senhaLogin = _senhaLogin;
                dnsHost = _dnsHost;
                DDNShost = _DDNShost;
                toleranciaHorario = _toleranciaHorario;
                funcaoLeitora = _funcaoLeitora;
                tempoLedBuzzerLeitoras = _tempoLedBuzzerLeitoras;
                toutPassback = _toutPassback;
                horarioVerao = _horarioVerao;
                porta5 = _porta5;
                porta6 = _porta6;
                porta7 = _porta7;
                porta8 = _porta8;
                porta9 = _porta9;
                porta10 = _porta10;
                porta11 = _porta11;
                porta12 = _porta12;
                porta13 = _porta13;
                tempoEventoEntradaDigitalAberta = _tempoEventoEntradaDigitalAberta;
                toutAlarmeEntradaDigitalAberta = _toutAlarmeEntradaDigitalAberta;
                toutAcionamentoLedBuzzer = _toutAcionamentoLedBuzzer;
                portaHttp = _portaHttp;
                parametro_01_Leitora = _parametro_01_Leitora;
                tLeitoraRS485_10ms = _tLeitoraRS485_10ms;
                funcaoRS485 = _funcaoRS485;
                tRele5 = _tRele5;
                horarioAtualizaAuto = _horarioAtualizaAuto;
                TempoQuedaCartao = _TempoQuedaCartao;
                tRele6 = _tRele6;


            }
        }

        //------------------------------------------------------------------------------------------------------------------
        // FLAGS_EVENTO
        public struct FLAGS_EVENTO
        {
            public byte bateriaFraca; //:1;
            public byte leitoraAcionada; //:2;
            public byte eventoLido; //:1; 
            public byte infoEvento; //:4

            public FLAGS_EVENTO(byte _bateriaFraca, byte _leitoraAcionada, byte _eventoLido, byte _infoEvento)
            {
                bateriaFraca = _bateriaFraca;
                leitoraAcionada = _leitoraAcionada;
                eventoLido = _eventoLido;
                infoEvento = _infoEvento;
            }
        }

        //------------------------------------------------------------------------------------------------------------------
        public struct s_evento
        {
            public byte tipoEvento;
            public byte modoOperacao; // bits 6 e 7
            public byte enderecoPlaca; // bits 0 a 5
            public byte serial0; // bits 4 a 7
            public byte tipoDispositivo; // bits 0 a 3
            public byte serial1;
            public byte serial2;
            public byte serial3;
            public byte serial4_contadorHCS;
            public byte serial5_contadorHCS;
            public byte hora;
            public byte minuto;
            public byte segundo;
            public byte dia;
            public byte mes;
            public byte ano;
            public byte nivel;
            FLAGS_EVENTO flagsEvento;

            public s_evento(byte _tipoEvento, byte _modoOperacao, byte _enderecoPlaca, byte _serial0, byte _tipoDispositivo, byte _serial1,
            byte _serial2, byte _serial3, byte _serial4_contadorHCS, byte _serial5_contadorHCS, byte _hora, byte _minuto, byte _segundo, byte _dia,
            byte _mes, byte _ano, byte _nivel, FLAGS_EVENTO _flagsEvento)
            {
                tipoEvento = _tipoEvento;
                modoOperacao = _modoOperacao;
                enderecoPlaca = _enderecoPlaca;
                serial0 = _serial0;
                tipoDispositivo = _tipoDispositivo;
                serial1 = _serial1;
                serial2 = _serial2;
                serial3 = _serial3;
                serial4_contadorHCS = _serial4_contadorHCS;
                serial5_contadorHCS = _serial5_contadorHCS;
                hora = _hora;
                minuto = _minuto;
                segundo = _segundo;
                dia = _dia;
                mes = _mes;
                ano = _ano;
                nivel = _nivel;
                flagsEvento = _flagsEvento;
            }
        }

        //------------------------------------------------------------------------------------------------------------------
        public struct MemoriaPlaca
        {
            public byte[,] dispositivos;
            public s_setup setup;
            public byte[,] rotas;
            public byte[,] gruposLeitoras;
            public byte[,] jornadas;
            public byte[,] turnos;
            public byte[] feriados;
            public byte[,] labelsRota;
            public byte[,] mensagensDisplay;

            public MemoriaPlaca(byte[,] _dispositivos, s_setup _setup, byte[,] _rotas, byte[,] _gruposLeitoras, byte[,] _jornadas, byte[,] _turnos, byte[] _feriados, byte[,] _labelsRota, byte[,] _mensagensDisplay)
            {
                dispositivos = _dispositivos;
                setup = _setup;
                rotas = _rotas;
                gruposLeitoras = _gruposLeitoras;
                jornadas = _jornadas;
                turnos = _turnos;
                feriados = _feriados;
                labelsRota = _labelsRota;
                mensagensDisplay = _mensagensDisplay;
            }
        }


        //------------------------------------------------------------------------------------------------------------------
        public struct MemoriaPlaca2
        {
            public EndPoint endPoint;
            public IPAddress ip;
            public int port;
            public Socket skt;
            public UInt16 indiceMsgRecebida;
            public byte[,] mensagemRecebida;


            public MemoriaPlaca2(EndPoint _endPoint, IPAddress _ip, int _port, Socket _skt, UInt16 _indiceMsgRecebida, byte[,] _mensagemRecebida)
            {
                endPoint = _endPoint;
                ip = _ip;
                port = _port;
                skt = _skt;
                indiceMsgRecebida = _indiceMsgRecebida;
                mensagemRecebida = _mensagemRecebida;
            }
        }

        //--------------------------------------------------------------
        //                          COMANDOS
        //--------------------------------------------------------------



        //--------------------------------------------------------------
        // 79:	Editar habilitação das leitoras	
        // CMD: 00 + 4F + <cód. da habilitação H> + <cód. da habilitação L> + <frame de habilitação (32 bytes)> + cs [37 bytes]
        // RSP: 00 + 4F + <retorno> + <cód. da habilitação H> + <cód. da habilitação L> + cs         [6 bytes]
        public byte[] editarHabilitacaoLeitoras(int codHabilitacao, byte[] bufHabilitacao)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_79);
            int i = 5, j = 0;

            buf[i++] = 0;
            buf[i++] = 79;
            buf[i++] = (byte)(codHabilitacao >> 8); // high
            buf[i++] = (byte)codHabilitacao; // low
            for (j = 0; j < SIZE_OF_HABILITACAO; j++) buf[i++] = bufHabilitacao[j];
            buf[i] = checksum(buf, N_BYTES_CMD_79);
            return buf;
        }

        //--------------------------------------------------------------
        // 80:	Gravar dispositivo	
        // CMD: 00 + 50 + <tipoDisp> + <serial 0> + <serial 1> + <serial 2> + <serial 3> + <cntH_s4> + <cntL_s5> + <cód. da habilitação H> + <cód. da habilitação L> +  <flagsCadastro> + <flagsStatus> + <nivel> + <créditos> + <validade[6bytes]> + 14x<userLabel> + cs          [36 bytes]
        // RSP: 00 + 50 + <retorno> + <tipoDisp> + <serial 0> + <serial 1> + <serial 2> + <serial 3> + <cntH_s4> + <cntL_s5> + cs         [11 bytes]
        public byte[] gravarDispositivo(byte tipo, UInt64 serial, int contadorHCS, int codHabilitacao, flagsCadastro fCadastro, flagsStatus fStatus, byte nivel, byte creditos, s_validade dataValidade, string userLabel)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_80);
            int i = 5, j = 0;
            buf[i++] = 0;
            buf[i++] = 80;
            //--------------


            buf[i++] = tipo;
            if (tipo == DISP_TX)
            {
                buf[i++] = (byte)(serial >> 24);
                buf[i++] = (byte)(serial >> 16);
                buf[i++] = (byte)(serial >> 8);
                buf[i++] = (byte)serial;

                buf[i++] = (byte)(contadorHCS >> 8);
                buf[i++] = (byte)contadorHCS;
            }
            else
            {
                buf[i++] = (byte)(serial >> 40);
                buf[i++] = (byte)(serial >> 32);
                buf[i++] = (byte)(serial >> 24);
                buf[i++] = (byte)(serial >> 16);
                buf[i++] = (byte)(serial >> 8);
                buf[i++] = (byte)serial;
            }

            buf[i++] = (byte)(codHabilitacao >> 8); // high
            buf[i++] = (byte)codHabilitacao; // low

            buf[i++] = fCadastro.val;
            buf[i++] = fStatus.val;

            buf[i++] = nivel;

            buf[i++] = creditos;

            buf[i++] = dataValidade.diaIni;
            buf[i++] = dataValidade.mesIni;
            buf[i++] = dataValidade.anoIni;
            buf[i++] = dataValidade.diaFim;
            buf[i++] = dataValidade.mesFim;
            buf[i++] = dataValidade.anoFim;

            for (j = 0; j < 14; j++)
            {
                if (userLabel.Length > j)
                {
                    buf[i++] = (byte)userLabel[j];
                }
                else
                {
                    buf[i++] = (byte)' ';
                }
            }


            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_80);
            return buf;
        }

        //-------------------------------------------------------------- 
        // 81	Editar dispositivo	
        // CMD: 00 + 51 + <tipoDisp> + <serial 0> + <serial 1> + <serial 2> + <serial 3> + <cntH_s4> + <cntL_s5> + <cód. da habilitação H> + <cód. da habilitação L> +  <flagsCadastro> + <flagsStatus> + <nivel> + <créditos> + <validade[6bytes]> + 14x<userLabel> + cs          [36 bytes]
        // RSP: 00 + 51 + <retorno> + <tipoDisp> + <serial 0> + <serial 1> + <serial 2> + <serial 3> + <cntH_s4> + <cntL_s5> + cs        [11 bytes]
        public byte[] editarDispositivo(byte tipo, UInt64 serial, int contadorHCS, int codHabilitacao, flagsCadastro fCadastro, flagsStatus fStatus, byte nivel, byte creditos, s_validade dataValidade, string userLabel)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_81);
            int i = 5, j = 0;
            buf[i++] = 0;
            buf[i++] = 81;
            //--------------
            buf[i++] = tipo;
            if (tipo == DISP_TX)
            {
                buf[i++] = (byte)(serial >> 24);
                buf[i++] = (byte)(serial >> 16);
                buf[i++] = (byte)(serial >> 8);
                buf[i++] = (byte)serial;

                buf[i++] = (byte)(contadorHCS >> 8);
                buf[i++] = (byte)contadorHCS;
            }
            else
            {
                buf[i++] = (byte)(serial >> 40);
                buf[i++] = (byte)(serial >> 32);
                buf[i++] = (byte)(serial >> 24);
                buf[i++] = (byte)(serial >> 16);
                buf[i++] = (byte)(serial >> 8);
                buf[i++] = (byte)serial;
            }

            buf[i++] = (byte)(codHabilitacao >> 8); // high
            buf[i++] = (byte)codHabilitacao; // low

            buf[i++] = fCadastro.val;
            buf[i++] = fStatus.val;

            buf[i++] = nivel;

            buf[i++] = creditos;

            buf[i++] = dataValidade.diaIni;
            buf[i++] = dataValidade.mesIni;
            buf[i++] = dataValidade.anoIni;
            buf[i++] = dataValidade.diaFim;
            buf[i++] = dataValidade.mesFim;
            buf[i++] = dataValidade.anoFim;

            for (j = 0; j < 14; j++)
            {
                if (userLabel.Length > j)
                {
                    buf[i++] = (byte)userLabel[j];
                }
                else
                {
                    buf[i++] = (byte)' ';
                }
            }

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_81);
            return buf;
        }

        //--------------------------------------------------------------
        // 82:	Apagar dispositivo
        // CMD: 00 + 52 + <tipoDisp> + <serial 0> + <serial 1> + <serial 2> + <serial 3> + <cntH_s4> + <cntL_s5> + cs                   [10 bytes]
        // RSP: 00 + 52 + <retorno> + <tipoDisp> + <serial 0> + <serial 1> + <serial 2> + <serial 3> + <cntH_s4> + <cntL_s5> + cs       [11 bytes]
        public byte[] apagarDispositivo(byte tipo, UInt64 serial)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_82);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 82;
            //--------------
            buf[i++] = tipo;

            if (tipo == DISP_TX)
            {
                buf[i++] = (byte)(serial >> 24);
                buf[i++] = (byte)(serial >> 16);
                buf[i++] = (byte)(serial >> 8);
                buf[i++] = (byte)serial;
                buf[i++] = 0;
                buf[i++] = 0;
            }
            else
            {
                buf[i++] = (byte)(serial >> 40);
                buf[i++] = (byte)(serial >> 32);
                buf[i++] = (byte)(serial >> 24);
                buf[i++] = (byte)(serial >> 16);
                buf[i++] = (byte)(serial >> 8);
                buf[i++] = (byte)serial;
            }

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_82);
            return buf;
        }

        //--------------------------------------------------------------
        // 83:	Formata memória	
        // CMD: 00 + 53 + cs          [3 bytes]
        // RSP: 00 + 53 + <retorno> + cs        [4 bytes]
        public byte[] formataMemoria()
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_83);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 83;
            buf[i] = checksum(buf, N_BYTES_CMD_83);
            return buf;
        }

        //--------------------------------------------------------------
        // 84	Contador de atualização do SETUP
        // CMD: 00 + 54 + <contadorAtualizaçãoSetup> + cs          [4 bytes]
        // RSP: 00 + 54 + <retorno> + cs       [4 bytes]
        public byte[] contadorAtualizacao(byte contadorAtualizacao)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_84);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 84;
            //--------------------
            buf[i++] = contadorAtualizacao;
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_84);
            return buf;
        }

        //--------------------------------------------------------------
        // 85	Editar turnos
        // CMD: 00 + 55 + <cód. do turno> + <frame de turno(16 bytes)> + cs         [20 bytes]
        // RSP: 00 + 55 + <retorno>  + <cód. do turno> + cs       [5 bytes]
        public byte[] editarTurnos(int codTurno, byte[] turnos)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_85);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 85;
            //--------------------
            buf[i++] = (byte)codTurno;
            for (j = 0; j < 16; j++) buf[i++] = turnos[j];
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_85);
            return buf;
        }

        /*
        //--------------------------------------------------------------
        // 86	Restore de dispositivo (PROGRESSIVO)
        // CMD: 00 + 56 + <tipoDisp> + <serial 0> + <serial 1> + <serial 2> + <serial 3> + <cntH_s4> + <cntL_s5> + <cód. da habilitação H> + <cód. da habilitação L> + <flagsCadastro> + <flagsStatus> + <nivel> + <créditos> + <validade[6bytes]> + 14x<userLabel> + cs         [36 bytes]
        // RSP: 00 + 56 + <retorno>  + cs       [4 bytes]
        public byte[] gravaDispositivo_PROGRESSIVO(byte tipo, UInt64 serial, int contadorHCS, int codHabilitacao, flagsCadastro fCadastro, flagsStatus fStatus, byte nivel, byte creditos, s_validade dataValidade, char[] userLabel)        
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_86);
            int i = 5, j = 0;
            buf[i++] = 0;
            buf[i++] = 86;
            //--------------
            buf[i++] = tipo;
            if (tipo == DISP_TX)
            {
                buf[i++] = (byte)(serial >> 24);
                buf[i++] = (byte)(serial >> 16);
                buf[i++] = (byte)(serial >> 8);
                buf[i++] = (byte)serial;

                buf[i++] = (byte)(contadorHCS >> 8);
                buf[i++] = (byte)contadorHCS;
            }
            else
            {
                buf[i++] = (byte)(serial >> 40);
                buf[i++] = (byte)(serial >> 32);
                buf[i++] = (byte)(serial >> 24);
                buf[i++] = (byte)(serial >> 16);
                buf[i++] = (byte)(serial >> 8);
                buf[i++] = (byte)serial;
            }

            buf[i++] = (byte)(codHabilitacao >> 8); // high
            buf[i++] = (byte)codHabilitacao; // low

            buf[i++] = fCadastro.val;
            buf[i++] = fStatus.val;

            buf[i++] = nivel;

            buf[i++] = creditos;

            buf[i++] = dataValidade.diaIni;
            buf[i++] = dataValidade.mesIni;
            buf[i++] = dataValidade.anoIni;
            buf[i++] = dataValidade.diaFim;
            buf[i++] = dataValidade.mesFim;
            buf[i++] = dataValidade.anoFim;

            for (j = 0; j < 14; j++) buf[i++] = (byte)userLabel[j];

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_86);
            return buf;
        }
        */

        //--------------------------------------------------------------
        // 86	Restore de dispositivo (PROGRESSIVO)
        // CMD: 00 + 56 + <framDisp(32 bytes) + <cs>
        // RSP: 00 + 56 + <retorno>  + cs       [4 bytes]
        public byte[] gravaDispositivo_PROGRESSIVO(byte[] frameDisp)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_86);
            int i = 5, j = 0;
            buf[i++] = 0;
            buf[i++] = 86;
            //--------------
            for (j = 0; j < SIZE_OF_DISPOSITIVO; j++) buf[i++] = frameDisp[j];

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_86);
            return buf;
        }

        //--------------------------------------------------------------
        // 87	Ler quantidade de dispositivos
        // CMD: 00 + 57 +  <tipo> + cs         [4 bytes]
        // RSP: 00 + 57 + <retorno> + <tipo> + <quantidade H> + <quantidade L> + cs       [7 bytes]
        public byte[] lerQuantidadeDispositivos(int tipo)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_87);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 87;
            //--------------------
            buf[i++] = (byte)tipo;
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_87);
            return buf;
        }

        //--------------------------------------------------------------
        // 88	Editar datas dos feriados
        // CMD: 00 + 58 + <frame datas dos feriados(48 bytes)> + cs         [51 bytes]
        // RSP: 00 + 58 + <retorno> + cs       [4 bytes]
        public byte[] editarDatasFeriados(byte[] feriados)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_88);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 88;
            //--------------------
            for (j = 0; j < N_LINES_FERIADO * SIZE_OF_FERIADO; j++) buf[i++] = feriados[j];
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_88);
            return buf;
        }

        //--------------------------------------------------------------
        // 89	Ler turnos	00 + 59 + <cód. do turno> + cs         [4 bytes]
        // CMD: 00 + 59 + <cód. do turno> + cs         [4 bytes]
        // RSP: 00 + 59 + <retorno> + <cód. do turno> + <frame de turno(16 bytes)> + cs       [21 bytes]
        public byte[] lerTurnos(int codTurnos)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_89);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 89;
            //--------------------
            buf[i++] = (byte)codTurnos;
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_89);
            return buf;
        }

        //--------------------------------------------------------------
        // 90	Ler quantidade de eventos	
        // CMD: 00 + 5A + <marca> + cs         [4 bytes]
        // RSP: 00 + 5A + <retorno> + <marca> + <qtdEventosH> + <qtdEventosL> + cs       [7 bytes]
        public byte[] lerQuantidadeEventos(int marca)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_90);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 90;
            //--------------------
            buf[i++] = (byte)marca;
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_90);
            return buf;
        }

        //--------------------------------------------------------------
        // 91	Ler / Gravar pacote de turnos (PACOTE DE DADOS)
        // CMD: 00 + 5B + <operacao> + 00 + <indice pacote turno (de 0 a 3)> + <pacote(512 bytes)> + cs          [518 bytes]
        // RSP: 00 + 5B + <retorno>  + <operacao> + 00 + <pacote(512 bytes)> + cs          [519 bytes]           (operação de escrita na PORTA_UDP2 = 00 + 5B + <retorno> + <operacao> + < indice pacote L> + <indice pacote H> + cs)   [7 bytes]
        public byte[] operacaoPacoteTurnos(byte operacao, UInt16 indice, byte[] pacoteDados)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_91);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 91;
            //--------------------
            buf[i++] = (byte)operacao;
            buf[i++] = (byte)(indice >> 8);
            buf[i++] = (byte)indice;
            for (j = 0; j < 512; j++)
            {
                if (operacao == OP_ESCRITA)
                {
                    buf[i++] = pacoteDados[j];
                }
                else
                {
                    buf[i++] = 0;
                }
            }
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_91);
            return buf;
        }

        //--------------------------------------------------------------
        // 92	Ler datas dos feriados
        // CMD: 00 + 5C + cs         [3 bytes]
        // RSP: 00 + 5C + <retorno> + <frame datas dos feriados(48 bytes)> + cs       [52 bytes]
        public byte[] lerDatasFeriados()
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_92);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 92;
            buf[i] = checksum(buf, N_BYTES_CMD_92);
            return buf;
        }

        //--------------------------------------------------------------
        // 93	Ler dispositivo (PROGRESSIVO)
        // CMD: 00 + 5D + cs         [3 bytes]
        // RSP: 00 + 5D + <retorno> + <tipoDisp> + <serial 0> + <serial 1> + <serial 2> + <serial 3> + <cntH_s4> + <cntL_s5> + <cód. da habilitação H> + <cód. da habilitação L> +  <flagsCadastro> + <flagsStatus> + <nivel> + <créditos> + <validade[6bytes]> + 14x<userLabel> + cs       [37 bytes]
        public byte[] lerDispositivo_PROGRESSIVO()
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_93);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 93;
            buf[i] = checksum(buf, N_BYTES_CMD_93);
            return buf;
        }

        //--------------------------------------------------------------
        // 94	Modo remoto 2
        // CMD: 00 + 5E + <tempo remoto> + <modo remoto> + cs         [5 bytes]
        // RSP: 00 + 5E + <retorno> + cs       [4 bytes]
        public byte[] modoRemoto(int tempoRemoto, int modo)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_94);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 94;
            //--------------

            buf[i++] = (byte)tempoRemoto;
            buf[i++] = (byte)modo;

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_94);
            return buf;
        }

        //--------------------------------------------------------------
        // 95	Ler Setup
        // 00 + 5F + cs         [3 bytes]
        // 00 + 5F + <retorno> + <setup(384bytes)> + cs       [388 bytes]
        public byte[] lerSetup()
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_95);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 95;
            buf[i] = checksum(buf, N_BYTES_CMD_95);
            return buf;
        }

        //--------------------------------------------------------------
        // 96	Grava Setup
        // CMD: 00 + 60 +  <setup(384 bytes)> + cs         [387 bytes]
        // RSP: 00 + 60 + <retorno> + cs       [4 bytes]
        public byte[] gravarSetup(s_setup setup)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_96);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 96;
            //----------------
            byte[] bufSetup = setupParaVetor(setup);
            for (j = 0; j < SIZE_OF_SETUP; j++) buf[i++] = bufSetup[j];
            //----------------
            buf[i] = checksum(buf, N_BYTES_CMD_96);
            return buf;

        }

        //--------------------------------------------------------------
        // 97	Escreve data e hora
        // CMD: 00 + 61 + <hora> + <min.> + <seg.> + <dia> + <mês> + <ano> + <cs>         [9 bytes]
        // RSP: 00 + 61 + <retorno> + cs       [4 bytes]
        public byte[] escreveDataHora(byte hora, byte minuto, byte segundo, byte dia, byte mes, int ano)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_97);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 97;
            //----------------
            buf[i++] = hora;
            buf[i++] = minuto;
            buf[i++] = segundo;
            buf[i++] = dia;
            buf[i++] = mes;
            if (ano > 99)
            {
                buf[i++] = (byte)(ano - 2000);
            }
            else
            {
                buf[i++] = (byte)ano;
            }
            //----------------
            buf[i] = checksum(buf, N_BYTES_CMD_97);
            return buf;
        }

        //--------------------------------------------------------------
        // 98	Ler data e hora
        // CMD: 00 + 62 + cs         [3 bytes]
        // RSP: 00 + 62 + <retorno> + <hora> + <min.> + <seg.> + <dia> + <mês> + <ano> + cs       [10bytes]
        public byte[] lerDataHora()
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_98);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 98;
            buf[i] = checksum(buf, N_BYTES_CMD_98);
            return buf;
        }

        //--------------------------------------------------------------
        // 99	Ler\Gravar pacote de dispositivos  (PACOTE DE DADOS)
        // CMD: 00 + 63 + <operacao> + <indicePacote_H(de 0 a 1)> + <indicePacote_L(de 0 a 255)> + <pacote(512 bytes)> + cs      [518 bytes]
        // RSP: 00 + 63 + <retorno> + <operacao> + <indice pacote (de 0 a 1)> + <indice pacote (de 0 a 255)> + <pacote(512 bytes)> + cs         [519 bytes]            (operação de escrita naPORTA_UDP2 = 00 + 63 + <retorno> + <operacao> + < indice pacote L> + <indice pacote H> + cs)   [7 bytes]
        public byte[] operacaoPacoteDispositivos(byte operacao, UInt16 indice, byte[] pacoteDados)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_99);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 99;
            //--------------------
            buf[i++] = (byte)operacao;
            buf[i++] = (byte)(indice >> 8);
            buf[i++] = (byte)indice;
            for (j = 0; j < 512; j++)
            {
                if (operacao == OP_ESCRITA)
                {
                    buf[i++] = pacoteDados[j];
                }
                else
                {
                    buf[i++] = 0;
                }
            }
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_99);
            return buf;
        }

        //--------------------------------------------------------------
        // 100	Atualiza antipassback especifico
        // CMD: 00 + 64 + <tipoDisp> + <serial 0> + <serial 1> + <serial 2> + <serial 3> + <s4> + <s5> + <nivel> + <estado Antipassback> + cs         [12 bytes]
        // RSP: 00 + 64 + <retorno> + cs       [4 bytes]
        public byte[] atualizarAntipassbackEspecifico(byte tipo, UInt64 serial, byte nivel, byte valorAntipassback)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_100);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 100;
            //--------------------
            buf[i++] = tipo;
            if (tipo == DISP_TX)
            {
                buf[i++] = (byte)(serial >> 24);
                buf[i++] = (byte)(serial >> 16);
                buf[i++] = (byte)(serial >> 8);
                buf[i++] = (byte)serial;

                buf[i++] = 0;
                buf[i++] = 0;
            }
            else
            {
                buf[i++] = (byte)(serial >> 40);
                buf[i++] = (byte)(serial >> 32);
                buf[i++] = (byte)(serial >> 24);
                buf[i++] = (byte)(serial >> 16);
                buf[i++] = (byte)(serial >> 8);
                buf[i++] = (byte)serial;
            }
            buf[i++] = nivel;
            buf[i++] = valorAntipassback;
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_100);
            return buf;
        }

        // 101	Editar Grupo da leitora (GL)
        // CMD: 00 + 65 + <códGrupoLeitoraH> +  <códGrupoLeitoraL> + <endCan> + <códGrupo(leitora1)> + <códGrupo(leitora2)> + <códGrupo(leitora3)> + <códGrupo(leitora4)> + cs         [10 bytes]
        // RSP: 00 + 65 + <retorno> + <códGrupoLeitoraH> +  <códGrupoLeitoraL> + cs       [6 bytes]
        public byte[] editarGrupoLeitoras(int codLeitoras, byte enderecoPlaca, byte codigoGrupoLeitora1, byte codigoGrupoLeitora2, byte codigoGrupoLeitora3, byte codigoGrupoLeitora4)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_101);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 101;
            //--------------

            buf[i++] = (byte)(codLeitoras >> 8);
            buf[i++] = (byte)codLeitoras;
            buf[i++] = enderecoPlaca;
            buf[i++] = codigoGrupoLeitora1;
            buf[i++] = codigoGrupoLeitora2;
            buf[i++] = codigoGrupoLeitora3;
            buf[i++] = codigoGrupoLeitora4;

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_101);
            return buf;
        }

        //--------------------------------------------------------------
        // 102	Ler Grupo da leitora (GL)
        // CMD: 00 + 66 + <códGrupoLeitoraH> + <códGrupoLeitoraL> + <endCan> + cs         [6 bytes]
        // RSP: 00 + 66 + <retorno> + <códGrupoLeitoraH> + <códGrupoLeitoraL> + <endCan> + <códGrupo(leitora1)> + <códGrupo(leitora2)> + <códGrupo(leitora3)> + <códGrupo(leitora4)> + cs       [11 bytes]
        public byte[] lerGrupoLeitoras(int codLeitoras, byte enderecoPlaca)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_102);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 102;
            //--------------

            buf[i++] = (byte)(codLeitoras >> 8);
            buf[i++] = (byte)codLeitoras;
            buf[i++] = enderecoPlaca;

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_102);
            return buf;
        }

        //--------------------------------------------------------------
        // 103	Cancela timout de PC
        // 00 + 67 + cs         [3 bytes]
        // 00 + 67 + <retorno> + cs       [4 bytes]
        public byte[] cancelaTimeoutProgressivo()
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_103);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 103;
            buf[i] = checksum(buf, N_BYTES_CMD_103);
            return buf;
        }

        //--------------------------------------------------------------
        // 104	Atualiza antipassback e nível (TODOS OS DISP. CADASTRADOS)
        // CMD: 00 + 68 + <nivel> + <estado Antipassback> + cs         [5 bytes]
        // RSP: 00 + 67 + <retorno> + cs       [4 bytes]
        public byte[] atualizaAntipassback_e_nivel(byte nivel, byte valorAntipassback)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_104);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 104;
            //--------------

            buf[i++] = nivel;
            buf[i++] = valorAntipassback;

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_104);
            return buf;
        }

        //--------------------------------------------------------------
        // 105	Ler eventos  (PROGRESSIVO)
        // CMD: 00 + 69 + <marca> + <cs>         [4 bytes]
        // RSP: 00 + 69 + <retorno> + <marca> + <frame de evt. (16 bytes)> + cs       [21 bytes]
        public byte[] lerEventos(byte marca)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_105);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 105;
            //--------------

            buf[i++] = marca;

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_105);
            return buf;
        }

        //--------------------------------------------------------------
        // 106	Gravar / Editar - Biometria / Identificar Template
        // CMD: 00 + 6A + <operacao>+  <indiceLeitora> + <tipoBiometria> + <frame dispositivo(32 bytes)> + <tamanhoTemplateH> + <tamanhoTemplateL> + <template> + cs         [40 bytes + template]
        // RSP: 00 + 6A + <retorno> + <operacao> + <indiceLeitora> + <serial 4> + <serial 5> + cs       [8 bytes]
        public byte[] operacoesBiometria1(byte operacao, byte indiceLeitora, byte tipoBiometria, byte[] frameDispositivo, UInt16 tamanhoTemplate, byte[] template)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_106 + tamanhoTemplate);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 106;
            //--------------

            buf[i++] = operacao;
            buf[i++] = indiceLeitora;
            buf[i++] = tipoBiometria;
            for (j = 0; j < SIZE_OF_DISPOSITIVO; j++) buf[i++] = frameDispositivo[j];
            buf[i++] = (byte)(tamanhoTemplate >> 8);
            buf[i++] = (byte)tamanhoTemplate;
            for (j = 0; j < tamanhoTemplate; j++) buf[i++] = template[j];

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_106 + tamanhoTemplate);
            return buf;
        }

        //--------------------------------------------------------------
        // 107	Apagar / Apagar Todos / Verificar ID gravado / ID vago - Biometria
        // CMD: 00 + 6B + <operacao> + <indiceLeitora> + <s4> + <s5> + cs         [7 bytes]
        // RSP: 00 + 6B + <retorno> + <operacao> + <indiceLeitora> + <serial 4> + <serial 5> + cs       [8 bytes]
        public byte[] operacoesBiometria2(byte operacao, byte indiceLeitora, UInt16 id)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_107);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 107;
            //--------------

            buf[i++] = operacao;
            buf[i++] = indiceLeitora;
            buf[i++] = (byte)(id >> 8);
            buf[i++] = (byte)id;

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_107);
            return buf;
        }

        //--------------------------------------------------------------
        // 108 Ler Biometria
        // 00 + 6C + <operacao> + <indiceLeitora> + <s4> + <s5> + cs
        // 00 + 6C + <retorno> + <operacao> +  <indiceLeitora> + <tipoBiometria> + <frame dispositivo(32 bytes)> + <tamanhoTemplateH> + <tamanhoTemplateL> + <template> + cs
        public byte[] lerBiometria(byte operacao, byte indiceLeitora, UInt16 id)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_108);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 108;
            //--------------

            buf[i++] = operacao;
            buf[i++] = indiceLeitora;
            buf[i++] = (byte)(id >> 8);
            buf[i++] = (byte)id;

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_108);
            return buf;
        }

        //--------------------------------------------------------------------------------------------------------
        // 109 Marca todos os eventos como lidos
        // 00 + 6D + cs
        // 00 + 6D + <retorno> + cs
        public byte[] todosEventosLidos()
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_109);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 109;
            buf[i] = checksum(buf, N_BYTES_CMD_109);
            return buf;
        }

        //--------------------------------------------------------------------------------------------------------
        // 110	Ler habilitação das Leitoras
        // 00 + 6E + <cód. da habilitação H> + <cód. da habilitação L> + cs 
        // 00 + 6E + <retorno> + <cód. da habilitação H> + <cód. da habilitação L> + <frame de habilitação(32 bytes)> + cs
        public byte[] lerHabilitacaoLeitoras(int codHabilitacao)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_110);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 110;
            //--------------

            buf[i++] = (byte)(codHabilitacao >> 8);
            buf[i++] = (byte)codHabilitacao;

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_110);
            return buf;
        }

        //--------------------------------------------------------------------------------------------------------
        // 111	Ler informações
        // 00 + 6F + <nDisp> + cs
        // 00 + 6F + <retorno> + <nDisp> +  <modo> + <DNS (16 bytes)> + <IP(4bytes)> + <versão SW (6bytes)> + <porta1 - UDP (2 bytes)> + <porta2 - UDP (2 bytes)> + <MAC ADDRESS(8 bytes)> + <cs>
        public byte[] lerInformacoes(byte enderecoPlaca)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_111);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 111;
            //--------------

            buf[i++] = (byte)enderecoPlaca;

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_111);
            return buf;
        }

        //--------------------------------------------------------------------------------------------------------
        // 112	Gravar/Ler contador atual de vagas
        // 00 + 70 + <operacao> + <vagas_atual_H> + <vagas_atual_L> + cs
        // 00 + 70 + <retorno> + <operacao> + <vagas_atual_H> + <vagas_atual_L> + cs
        public byte[] gravarLerContadorAtualVagas(byte operacao, UInt16 contadorVagas)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_112);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 112;
            //--------------

            buf[i++] = (byte)operacao;
            buf[i++] = (byte)(contadorVagas >> 8);
            buf[i++] = (byte)contadorVagas;

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_112);
            return buf;
        }

        //--------------------------------------------------------------------------------------------------------
        // 113	Ler / Gravar pacote de jornada  (PACOTE DE DADOS)
        // 00 + 71 + <operacao> + 00 + 00 + <pacote(512 bytes)> + cs        [518 bytes]
        // 00 + 71 + <retorno>  + <operacao> + 00 + <pacote(512 bytes)> + cs          [519 bytes]
        //           (operação de escrita na PORTA_UDP2 = 00 + 5B + <retorno> + <operacao> + < indice pacote L> + <indice pacote H> + cs)   [7 bytes]
        public byte[] operacaoPacoteJornada(byte operacao, UInt16 indice, byte[] pacoteDados)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_113);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 113;
            //--------------------
            buf[i++] = (byte)operacao;
            buf[i++] = (byte)(indice >> 8);
            buf[i++] = (byte)indice;
            for (j = 0; j < 512; j++)
            {
                if (operacao == OP_ESCRITA)
                {
                    buf[i++] = pacoteDados[j];
                }
                else
                {
                    buf[i++] = 0;
                }
            }
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_113);
            return buf;
        }

        //--------------------------------------------------------------------------------------------------------
        // 114 - Edita jornada
        // 00 + 72 + <cód. Jornada> + <cód. turno segunda>  + <cód. turno terça> + <cód. turno quarta> + <cód. turno quinta> + <cód. turno sexta> + <cód. turno sábado> + <cód. turno domingo> + <flagsJornada2> + cs
        // 00 + 72 + <retorno> + <cód. Jornada> + cs
        public byte[] editarJornada(byte codigoJornada, byte codigoTurno_segunda, byte codigoTurno_terca, byte codigoTurno_quarta, byte codigoTurno_quinta, byte codigoTurno_sexta, byte codigoTurno_sabado, byte codigoTurno_domingo, byte codigoTurno_feriado)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_114);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 114;
            //--------------

            buf[i++] = codigoJornada;
            buf[i++] = codigoTurno_segunda;
            buf[i++] = codigoTurno_terca;
            buf[i++] = codigoTurno_quarta;
            buf[i++] = codigoTurno_quinta;
            buf[i++] = codigoTurno_sexta;
            buf[i++] = codigoTurno_sabado;
            buf[i++] = codigoTurno_domingo;
            buf[i++] = codigoTurno_feriado;

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_114);
            return buf;
        }

        //--------------------------------------------------------------------------------------------------------
        // 115	Ler jornada
        // 00 + 73 + <cód. Jornada> + cs         [4 bytes]
        // 00 + 73 + <retorno> + <cód. Jornada> + <cód. turno segunda>  + <cód. turno terça> + <cód. turno quarta> + <cód. turno quinta> + <cód. turno sexta> + <cód. turno sábado> + <cód. turno domingo> + <cód. turno feriados> + cs       [13 bytes]
        public byte[] lerJornada(byte codigoJornada)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_115);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 115;
            //--------------

            buf[i++] = codigoJornada;

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_115);
            return buf;
        }

        // 116 - Evento automático
        // ---
        // 00 + 74 + <retorno> + <cntAtual> + <frame de evt. (16 bytes)> + cs      [21 bytes]
        public void vetorParaEvento(byte[] v, ref byte contadorAtual, ref byte evento, ref byte modoOperacao, ref byte endereco, ref byte tipoDispositivo, ref UInt64 serial, ref UInt16 contadorHCS, ref byte dia, ref byte mes, ref byte ano, ref byte hora, ref byte minuto, ref byte segundo, ref byte nivel, ref FLAGS_EVENTO flagsEvento, ref byte setor)
        {
            contadorAtual = v[3];
            evento = (byte)(v[4] & 0x1F);
            setor = (byte)(v[4] >> 5);
            modoOperacao = (byte)(v[5] >> 6);
            endereco = (byte)(v[5] & 0x3F);
            tipoDispositivo = (byte)(v[6] & 0x0F);
            if (tipoDispositivo == DISP_TX)
            {
                serial = (UInt64)((v[6] >> 4) << 24);
                serial += (UInt64)((v[7]) << 16);
                serial += (UInt64)((v[8]) << 8);
                serial += (UInt64)(v[9]);
                contadorHCS = (UInt16)((v[10]) << 8);
                contadorHCS += (UInt16)(v[11]);
            }
            else
            {
                serial = (UInt64)((UInt64)(v[6] >> 4) << 40);
                serial += (UInt64)((UInt64)(v[7]) << 32);
                serial += (UInt64)((UInt64)(v[8]) << 24);
                serial += (UInt64)((UInt64)(v[9]) << 16);
                serial += (UInt64)((UInt64)(v[10]) << 8);
                serial += (UInt64)(v[11]);
            }

            hora = v[12];
            minuto = v[13];
            segundo = v[14];
            dia = v[15];
            mes = v[16];
            ano = v[17];

            nivel = v[18];
            flagsEvento.bateriaFraca = (byte)(v[19] & 0x01);
            flagsEvento.leitoraAcionada = (byte)((v[19] >> 1) & 0x03);
            flagsEvento.eventoLido = (byte)((v[19] >> 3) & 0x01);
            flagsEvento.infoEvento = (byte)((v[19] >> 4) & 0x0F);
        }



        //--------------------------------------------------------------------------------------------------------
        // 117 - Acionamento
        // 00 + 75 + <dispositivo> + <nDisp> + <saida> + cs
        // 00 + 75 + <retorno> + <dispositivo> + <nDisp> + <saida> + cs
        public byte[] acionamento(byte dispositivo, byte enderecoPlaca, byte saida)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_117);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 117;
            //--------------

            buf[i++] = dispositivo;
            buf[i++] = enderecoPlaca;
            buf[i++] = saida;

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_117);
            return buf;
        }

        //--------------------------------------------------------------------------------------------------------
        // 118 - Ler dispositivo
        // 00 + 76 + <tipoDisp> + <serial 0> + <serial 1> + <serial 2> + <serial 3> + <cntH_s4> + <cntL_s5> + cs	
        // 00 + 76 + <retorno> + <tipoDisp> + <serial 0> + <serial 1> + <serial 2> + <serial 3> + <cntH_s4> + <cntL_s5> + <cód. da habilitação H> + <cód. da habilitação L> + <flagsCadastro> + <flagsStatus> + 3x<reservado> + <nivel> + <validade/créditos> + 17x<userLabel> + cs	
        public byte[] lerDispositivo(byte tipo, UInt64 serial)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_118);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 118;
            //--------------
            buf[i++] = tipo;
            if (tipo == DISP_TX)
            {
                buf[i++] = (byte)(serial >> 24);
                buf[i++] = (byte)(serial >> 16);
                buf[i++] = (byte)(serial >> 8);
                buf[i++] = (byte)serial;

                buf[i++] = 0;
                buf[i++] = 0;
            }
            else
            {
                buf[i++] = (byte)(serial >> 40);
                buf[i++] = (byte)(serial >> 32);
                buf[i++] = (byte)(serial >> 24);
                buf[i++] = (byte)(serial >> 16);
                buf[i++] = (byte)(serial >> 8);
                buf[i++] = (byte)serial;
            }

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_118);
            return buf;
        }

        //--------------------------------------------------------------------------------------------------------
        // 119	Bypass
        // 00 + 77 + <bypassOn> + <dispositivo> + <tamanhoComandoH>+ <tamanhoComandoL> + <frameComando> + cs
        // 00 + 77 + <retorno> + <bypassOn> + <dispositivo> + <tamanhoRespostaH>+ <tamanhoRespostaL> + <frameComando> + cs
        public byte[] bypass(bool bypass, byte dispositivo, UInt16 tamanhoComando, byte[] frameComando)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_119);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 119;
            //--------------

            if (bypass == true) { buf[i++] = 1; } else { buf[i++] = 0; }
            buf[i++] = dispositivo;
            buf[i++] = (byte)(tamanhoComando >> 8);
            buf[i++] = (byte)tamanhoComando;
            for (j = 0; j < tamanhoComando; j++) buf[i++] = frameComando[j];

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_119);
            return buf;
        }

        //--------------------------------------------------------------------------------------------------------
        // 120	Edita parâmetro Setup	
        // 00 + 78 + <operacao> + <parametroSetupH> + <parametroSetupL> + <tamanhoParametroH> + <tamanhoParametroL> + <parametro> + cs
        // 00 + 78 + <retorno> + <operacao> + <parametroSetupH> + <parametroSetupL> + <tamanhoParametroH> + <tamanhoParametroL> + <parametro> + cs
        public byte[] operacaoParametroSetup(byte operacao, UInt16 indiceParametro, UInt16 tamanhoParametro, byte[] parametro)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_120 + tamanhoParametro);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 120;
            //--------------

            buf[i++] = operacao;
            buf[i++] = (byte)(indiceParametro >> 8);
            buf[i++] = (byte)indiceParametro;
            buf[i++] = (byte)(tamanhoParametro >> 8);
            buf[i++] = (byte)tamanhoParametro;
            for (j = 0; j < tamanhoParametro; j++) buf[i++] = parametro[j];

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_120 + tamanhoParametro);
            return buf;
        }


        //--------------------------------------------------------------------------------------------------------
        // 121	Ler/Edita habilitação das Leitoras individual
        // 00 + 79 + <operação> + <cód. da habilitação H> + <cód. da habilitação L> + <nDisp> + <habilitação> + <labelRota(8bytes)>+ cs
        // 00 + 79 + <retorno> + <operação> + <cód. da habilitação H> + <cód. da habilitação L> + <nDisp> + <habilitação> + <labelRota(8bytes)>+  cs 
        public byte[] operacaoRotaIndividual(byte operacao, UInt16 codigoRota, byte enderecoPlaca, byte bHabilitacao, String labelRota)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_121);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 121;
            //--------------

            buf[i++] = operacao;
            buf[i++] = (byte)(codigoRota >> 8);
            buf[i++] = (byte)codigoRota;
            buf[i++] = enderecoPlaca;
            buf[i++] = bHabilitacao;
            for (j = 0; j < 8; j++)
            {
                if (j < labelRota.Length)
                {
                    buf[i++] = (byte)labelRota[j];
                }
                else
                {
                    buf[i++] = (byte)' ';
                }
            }

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_121);
            return buf;
        }

        //--------------------------------------------------------------------------------------------------------
        // 122	Edita data de feriado individual
        // 00 + 7A + <cód. do feriado> + <dia> + <mês> + cs
        // 00 + 7A + <retorno> + <cód. do feriado> + cs
        public byte[] editaDataFeriadoIndividual(byte indiceFeriado, byte dia, byte mes)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_122);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 122;
            //--------------

            buf[i++] = indiceFeriado;
            buf[i++] = dia;
            buf[i++] = mes;

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_122);
            return buf;
        }

        //--------------------------------------------------------------------------------------------------------
        // 123	Ler/Editar mensagens display externo
        // 00 + 7B + <operação> + <cód. MsgDisplayExterno> + <mensagemDisplayExterno(32bytes)> + cs
        // 00 + 7B + <retorno> + <operação> +  <cód. MsgDisplayExterno> + <mensagemDisplayExterno(32bytes)> + cs		
        public byte[] operacaoMensagensDisplayExterno(byte operacao, byte indiceMensagem, String linha1, String linha2)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_123);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 123;
            //--------------

            buf[i++] = operacao;
            buf[i++] = indiceMensagem;

            if (operacao == OP_LEITURA)
            {
                for (j = 0; j < 32; j++) buf[i++] = (byte)' ';
            }
            else
            {
                for (j = 0; j < 16; j++)
                {
                    if (j < linha1.Length)
                    {
                        buf[i++] = Convert.ToByte(linha1[j]);
                    }
                    else
                    {
                        buf[i++] = (byte)' ';
                    }
                }

                for (j = 0; j < 16; j++)
                {
                    if (j < linha2.Length)
                    {
                        buf[i++] = Convert.ToByte(linha2[j]);
                    }
                    else
                    {
                        buf[i++] = (byte)' ';
                    }
                }
            }

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_123);
            return buf;
        }

        //--------------------------------------------------------------------------------------------------------
        // 124	Ler / Gravar pacote de grupos de leitora (GL)  (PACOTE DE DADOS)
        // 00 + 7C + <operacao> + 00 + <indicePacote_L (de 0 a 207)> + <pacote(512 bytes)> + cs          [518 bytes]
        // 00 + 7C + <retorno> + <operacao> + 00 + <indice pacote (de 0 a 207)> + <pacote(512 bytes)> + cs          [519 bytes]
        //            (operação de escrita na PORTA_UDP2 = 00 + 7C + <retorno> + 00 + < indice pacote L> + <indice pacote H> + cs)   [7 bytes]
        public byte[] operacaoPacoteGrupoLeitoras(byte operacao, UInt16 indice, byte[] pacoteDados)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_124);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 124;
            //--------------------
            buf[i++] = (byte)operacao;
            buf[i++] = (byte)(indice >> 8);
            buf[i++] = (byte)indice;
            for (j = 0; j < 512; j++)
            {
                if (operacao == OP_ESCRITA)
                {
                    buf[i++] = pacoteDados[j];
                }
                else
                {
                    buf[i++] = 0;
                }
            }
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_124);
            return buf;
        }

        //--------------------------------------------------------------------------------------------------------
        // 125	Ler / Gravar pacote de habilitação  (PACOTE DE DADOS)           [518 bytes]
        // 00 + 7D + <operacao> + 00 + <indicePacote_L(de 0 a 25)> + <pacote(512 bytes)> + cs
        // 00 + 7D + <retorno> + <operacao> + 00 + <indice pacote (de 0 a 25)> + <pacote(512 bytes)> + cs          [519 bytes]
        //           (operação de escrita na PORTA_UDP2 = 00 + 7D + <retorno> + 00 + < indice pacote L> + <indice pacote H> + cs)   [7 bytes]
        public byte[] operacaoPacoteHabilitacao(byte operacao, UInt16 indice, byte[] pacoteDados)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_125);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 125;
            //--------------------
            buf[i++] = (byte)operacao;
            buf[i++] = (byte)(indice >> 8);
            buf[i++] = (byte)indice;
            for (j = 0; j < 512; j++)
            {
                if (operacao == OP_ESCRITA)
                {
                    buf[i++] = pacoteDados[j];
                }
                else
                {
                    buf[i++] = 0;
                }
            }
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_125);
            return buf;
        }
        //--------------------------------------------------------------------------------------------------------
        // 126	Ler / Gravar pacote de Labels de rota  (PACOTE DE DADOS)
        // 00 + 7E + <operacao> + 00 + <indicePacote_L (de 0 a 1)> + <pacote(512 bytes)> + cs          [518 bytes]
        // 00 + 7E + <retorno> + <operacao> + 00 + <indice pacote (de 0 a 1)> + <pacote(512 bytes)> + cs          [519 bytes]
        //           (operação de escrita na PORTA_UDP2 = 00 + 7E + <retorno> + 00 + < indice pacote L> + <indice pacote H> + cs)   [7 bytes]
        public byte[] operacaoPacoteLabelsRota(byte operacao, UInt16 indice, byte[] pacoteDados)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_126);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 126;
            //--------------------
            buf[i++] = (byte)operacao;
            buf[i++] = (byte)(indice >> 8);
            buf[i++] = (byte)indice;
            for (j = 0; j < 512; j++)
            {
                if (operacao == OP_ESCRITA)
                {
                    buf[i++] = pacoteDados[j];
                }
                else
                {
                    buf[i++] = 0;
                }
            }
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_126);
            return buf;
        }

        //--------------------------------------------------------------------------------------------------------
        // 127	Grava configuração de rede no MAC ADDRESS especificado
        // 00 + 7F + <MAC ADDRESS(6 bytes)> + <DNS (16 bytes)> + <IP(4bytes)> + <porta1 - UDP (2 bytes)> + <porta2 - UDP (2 bytes)> + <cs>  [33 bytes]
        // 00 + 7F + <retorno> + <MAC ADDRESS(6 bytes)> + <DNS (16 bytes)> + <IP(4bytes)> + <porta1 - UDP (2 bytes)> + <porta2 - TCP (2 bytes)> + <cs>   [34 bytes]
        public byte[] gravaConfiguracaoNoMacAddress(byte[] macAddress_destino, byte[] new_hostName, byte[] new_ip, UInt16 new_portaUDP, UInt16 new_portaTCPClient)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_127);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 127;
            //--------------------
            for (j = 0; j < 6; j++) buf[i++] = (byte)macAddress_destino[j];
            for (j = 0; j < SIZE_OF_DNS_HOST; j++) buf[i++] = (byte)new_hostName[j];
            for (j = 0; j < 4; j++) buf[i++] = (byte)new_ip[j];
            buf[i++] = (byte)(new_portaUDP >> 8);
            buf[i++] = (byte)new_portaUDP;
            buf[i++] = (byte)(new_portaTCPClient >> 8);
            buf[i++] = (byte)new_portaTCPClient;
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_127);
            return buf;
        }

        //--------------------------------------------------------------------------------------------------------
        // 128	
        // Editar habilitação e grupo da leitora
        // 00 + 80 + <operacao> + <cód. da rota H> + <cód. da rota L> + <nDisp> + <habilitação das 4 leitoras(bit3/bit2/bit1/bit0)> +
        //         <códGrupo(leitora1)> + <códGrupo(leitora2)> + <códGrupo(leitora3)> + <códGrupo(leitora4)> + <labelRota(8bytes)> + cs      [20 bytes]
        // 00 + 80 + <retorno> + <operacao> + <cód. da rota H> + <cód. da rota L> + <nDisp> + <habilitação das 4 leitoras(bit3/bit2/bit1/bit0)> + 
        //         <códGrupo(leitora1)> + <códGrupo(leitora2)> + <códGrupo(leitora3)> + <códGrupo(leitora4)> + <labelRota(8bytes)> + cs      [21 bytes]
        public byte[] operacaoRotaGrupoLeitoras(byte operacao, UInt16 codigoRota, byte enderecoPlaca, byte habilitacaoLeitoras,
                                                byte codigoGrupoLeitora1, byte codigoGrupoLeitora2, byte codigoGrupoLeitora3, byte codigoGrupoLeitora4,
                                                String labelRota)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_128);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 128;
            //--------------------
            buf[i++] = operacao;
            buf[i++] = (byte)(codigoRota >> 8);
            buf[i++] = (byte)codigoRota;
            buf[i++] = enderecoPlaca;
            buf[i++] = habilitacaoLeitoras;
            buf[i++] = codigoGrupoLeitora1;
            buf[i++] = codigoGrupoLeitora2;
            buf[i++] = codigoGrupoLeitora3;
            buf[i++] = codigoGrupoLeitora4;
            for (j = 0; j < 8; j++)
            {
                if (j < labelRota.Length)
                {
                    buf[i++] = Convert.ToByte(labelRota[j]);
                }
                else
                {
                    buf[i++] = Convert.ToByte(' ');
                }
            }
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_128);
            return buf;
        }


        //--------------------------------------------------------------------------------------------------------
        // 129	Ler dados do SD CARD (PROGRESSIVO)
        // 00 + 81 + <arquivo sdcard> + <dia> + <mês> + <ano> + <hora> + <minutos> + <segundos> + cs       [10 bytes]
        // 00 + 81 + <retorno> + <arquivo sdcard> + <dia> + <mês> + <ano> + <hora> + <minutos> + <segundos> +  <tamanhoLinhaH> + tamanhoLinhaL> + <linha do arquivo> + cs    [13 bytes + linha do arquivo]
        public byte[] lerDadosSDCARD(byte arquivoSDCARD, byte dia, byte mes, byte ano, byte hora, byte minuto, byte segundo)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_129);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 129;
            //--------------------
            buf[i++] = arquivoSDCARD;
            buf[i++] = dia;
            buf[i++] = mes;
            buf[i++] = ano;
            buf[i++] = hora;
            buf[i++] = minuto;
            buf[i++] = segundo;
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_129);
            return buf;
        }

        //--------------------------------------------------------------------------------------------------------
        // 130 - Apaga último evento
        // 00 + 82 + 82
        // 00 + 82 + <retorno> + cs    [4 bytes]
        public byte[] apagarUltimoEvento()
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_130);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 130;
            //--------------
            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_130);
            return buf;
        }
        //--------------------------------------------------------------------------------------------------------
        // 131	Ler / Gravar pacote de Vagas por Habilitação  (PACOTE DE DADOS)	
        // CMD: 00 + 83 + <operacao> + 00 + <indicePacote_L(de 0 a 3)> + <pacote(512 bytes)> + cs  [518 bytes]
        // RSP:	00 + 83 + <retorno> + <operacao> + 00 + <indice pacote (de 0 a 3)> + <pacote(512 bytes)> + cs [519 bytes]
        public byte[] operacaoPacoteVagasHabilitação(byte operacao, UInt16 indice, byte[] pacoteDados)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_131);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 131;
            //--------------------
            buf[i++] = (byte)operacao;
            buf[i++] = (byte)(indice >> 8);
            buf[i++] = (byte)indice;
            for (j = 0; j < 512; j++)
            {
                if (operacao == OP_ESCRITA)
                {
                    buf[i++] = pacoteDados[j];
                }
                else
                {
                    buf[i++] = 0;
                }
            }
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_131);
            return buf;
        }


        //--------------------------------------------------------------------------------------------------------
        // 132 Acionamento remoto com identificação
        // 00 + 84 + <dispositivo> + <nDisp> + <saída> + <tipoDisp> + <serial 0> + <serial 1> + <serial 2> +
        //         <serial 3> + <serial 4/contadorH> + <serial 5/contadorL> + <flagsCadastro> + <nivel> + cs
        // 00 + 84 + <retorno> + cs   [4 bytes]
        public byte[] acionamentoComIdentificacao(byte dispositivo, byte enderecoPlaca, byte saida, byte tipo, UInt64 serial, UInt16 contadorHCS,
                                                   flagsCadastro fCadastro, byte nivel, byte origemAcionamento)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_132);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 132;
            //--------------

            buf[i++] = dispositivo;
            buf[i++] = enderecoPlaca;
            buf[i++] = saida;
            buf[i++] = tipo;

            if (tipo == DISP_TX)
            {
                buf[i++] = (byte)((serial >> 24) & 0x0F);
                buf[i++] = (byte)(serial >> 16);
                buf[i++] = (byte)(serial >> 8);
                buf[i++] = (byte)serial;
                buf[i++] = (byte)(contadorHCS >> 8);
                buf[i++] = (byte)contadorHCS;
            }
            else
            {
                buf[i++] = (byte)(serial >> 40);
                buf[i++] = (byte)(serial >> 32);
                buf[i++] = (byte)(serial >> 24);
                buf[i++] = (byte)(serial >> 16);
                buf[i++] = (byte)(serial >> 8);
                buf[i++] = (byte)serial;
            }

            buf[i++] = fCadastro.val;
            buf[i++] = nivel;
            buf[i++] = origemAcionamento;

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_132);
            return buf;
        }

        //--------------------------------------------------------------------------------------------------------
        // 133	Ler evento/Marcar evento como lido/Ler último evento não lido (EVENTO INDEXADO)
        // CMD: 00 + 85 + <operacao> + <endereçoEventoH> + <endereçoEventoL> + cs     [6 bytes]
        // RSP: 00 + 85 + <retorno> + <operacao> + <cntAtual> + <frame Evento(16 bytes)> + <endereçoEventoH> + <endereçoEventoL> + cs    [24 bytes]
        public byte[] operacaoEvento(byte operacao, UInt16 enderecoEvento)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_133);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 133;
            //--------------
            buf[i++] = operacao;
            buf[i++] = (byte)(enderecoEvento >> 8);
            buf[i++] = (byte)(enderecoEvento);
            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_133);
            return buf;
        }
        //--------------------------------------------------------------------------------------------------------
        // 134	
        // 
        //  EVENTO AUTOMATICO INDEXADO

        //--------------------------------------------------------------------------------------------------------
        // 135 Leitura de evento com endereço do ponteiro (EVENTO INDEXADO em PACOTE DE DADOS)
        // 00 + 87 + <indice_H> + <indice_L> + cs [5 bytes] (indice 0 a 15 => 8192 EVENTOS / 512   )
        // 00 + 87 + <retorno> + <indice_H> + <indice_L> + < 32 x (frame de evt. (16 bytes) + <endereçoEventoH> + <endereçoEventoL>)> + cs [ 582 bytes]
        public byte[] lerPacoteEventos(UInt16 indice)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_135);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 135;
            //--------------------
            buf[i++] = (byte)(indice >> 8);
            buf[i++] = (byte)(indice);
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_135);
            return buf;
        }

        //--------------------------------------------------------------------------------------------------------
        // 136 Ler antipassback específico
        // CMD: 00 + 88 + <endereco da Placa(0 a 63)> + <tipoDisp> + <S0> + <S1> + <S2> + <S3> + <S4> + <S5> + cs   [11 bytes]
        // RSP:	00 + 87 + <retorno> + <endereco da Placa(0 a 63)> + <tipoDisp> + <S0> + <S1> + <S2> + <S3> + <S4> + <S5> + <estado Antipassback> + nível + cs   [14 bytes]
        public byte[] lerAntipassbackEspecifico(byte endereco, byte tipo, UInt64 serial)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_136);
            int i = 5;

            buf[i++] = 0;
            buf[i++] = 136;
            //--------------
            buf[i++] = endereco;
            buf[i++] = tipo;
            buf[i++] = (byte)(serial >> 40);
            buf[i++] = (byte)(serial >> 32);
            buf[i++] = (byte)(serial >> 24);
            buf[i++] = (byte)(serial >> 16);
            buf[i++] = (byte)(serial >> 8);
            buf[i++] = (byte)serial;

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_136);
            return buf;
        }

        //--------------------------------------------------------------------------------------------------------
        // 137	Enviar mensagem para do display externo
        // 00 + 89 + <00(reservado)> + <tamanhoMensagem> + <mensagem(ASCII)> + cs          [5 bytes + mensagem]
        // 00 + 89 + <retorno> + cs          [4 bytes]
        public byte[] enviarMensagemDisplayExterno(byte tamnahoMensagem, String mensagem)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_137 + tamnahoMensagem);
            int i = 5, j;

            buf[i++] = 0;
            buf[i++] = 137;
            //--------------
            buf[i++] = 0;
            buf[i++] = tamnahoMensagem;
            for (j = 0; j < tamnahoMensagem; j++) buf[i++] = (byte)mensagem[j];

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_137 + tamnahoMensagem);
            return buf;
        }
        //--------------------------------------------------------------------------------------------------------
        // 138	Ler / Gravar Vagas por Habilitação	
        // CMD: 00 + 8A + <operação> + <codHAB(High)> +  <codHAB(Low)> + <total de vagas disponível (preset vagas) (0 a 255)> + <quantidade atual de vagas(0 a 255)> + cs              [8 bytes]
        // RSP:	00 + 8A + <retorno> + <operação> + <codHAB(High)> +  <codHAB(Low)> + <total de vagas disponível (preset vagas) (0 a 255)> + <quantidade atual de vagas(0 a 255)> + cs  [9 bytes] 
        public byte[] operacaoVagasHabilitacao(byte operacao, UInt16 codHab, byte totalVagas, byte vagasAtual)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_138);
            int i = 5;

            buf[i++] = 0;
            buf[i++] = 138;
            //--------------
            buf[i++] = operacao;
            buf[i++] = (byte)(codHab >> 8);
            buf[i++] = (byte)codHab;
            buf[i++] = totalVagas;
            buf[i++] = vagasAtual;

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_138);
            return buf;
        }
        //--------------------------------------------------------------------------------------------------------
        // 139	Restaurar Vagas por Habilitação para o valor de Preset	
        // CMD: 00 + 8B + <codHAB(High)> +  <codHAB(Low)> + cs                [5 bytes]
        // RSP: 00 + 8B + <retorno> + <codHAB(High)> +  <codHAB(Low)> + cs    [6 bytes]	
        public byte[] restaurarVagasHabilitacao(UInt16 codHab)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_139);
            int i = 5;

            buf[i++] = 0;
            buf[i++] = 139;
            //--------------
            buf[i++] = (byte)(codHab >> 8);
            buf[i++] = (byte)codHab;

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_139);
            return buf;
        }
        //--------------------------------------------------------------------------------------------------------
        // 140	Ler Vagas especifico
        // CMD: 00 + 8C + <tipoDisp> + <serial S0> + <serial S1> + <serial S2> + <serial S3> + <serial S4> + <serial S5> + cs           [10 bytes]	
        // RSP: 00 + 8C + <retorno> + <tipoDisp> + <serial S0> + <serial S1> + <serial S2> + <serial S3> + <serial S4> + <serial S5> + <codHAB(High)> + <codHAB(Low)> + <total de vagas disponível (preset vagas) (0 a 255)> + <quantidade atual de vagas(0 a 255)> + cs  [15 bytes]
        public byte[] lerVagasEspecifico(byte tipo, UInt64 serial)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_140);
            int i = 5;

            buf[i++] = 0;
            buf[i++] = 140;
            //--------------------
            buf[i++] = tipo;
            if (tipo == DISP_TX)
            {
                buf[i++] = (byte)(serial >> 24);
                buf[i++] = (byte)(serial >> 16);
                buf[i++] = (byte)(serial >> 8);
                buf[i++] = (byte)serial;

                buf[i++] = 0;
                buf[i++] = 0;
            }
            else
            {
                buf[i++] = (byte)(serial >> 40);
                buf[i++] = (byte)(serial >> 32);
                buf[i++] = (byte)(serial >> 24);
                buf[i++] = (byte)(serial >> 16);
                buf[i++] = (byte)(serial >> 8);
                buf[i++] = (byte)serial;
            }
            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_140);
            return buf;
        }
        //--------------------------------------------------------------------------------------------------------
        // 141	Marcar PACOTE de evento como "lido" a partir do endereço do evento (EVENTO INDEXADO em PACOTE DE DADOS)	
        // CMD: 00 + 8D + <endereçoEventoH> + <endereçoEventoL> + cs [5 bytes]
        // RSP: 00 + 8D + <retorno> + <endereçoEventoH> + <endereçoEventoL> + cs [6 bytes]
        public byte[] marcarPacoteEventoIndexadoComoLido(UInt16 enderecoEvento)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_141);
            int i = 5;

            buf[i++] = 0;
            buf[i++] = 141;
            //--------------------
            buf[i++] = (byte)(enderecoEvento >> 8);
            buf[i++] = (byte)(enderecoEvento);
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_141);
            return buf;
        }
        //--------------------------------------------------------------------------------------------------------
        // 142	Ler habilitação das leitoras 2 (c/ SETOR)	
        // CMD: 00 + 8E + <cód. da habilitação H> +  <cód. da habilitação L> + <setor> + cs         											   		[6 bytes]
        // RSP: 00 + 8E + <retorno> + <cód. da habilitação H> +  <cód. da habilitação L> + <frame de habilitação (256 bytes)> + <setor> + cs  [263 bytes] 
        public byte[] lerHabilitacaoLeitorasComSetor(int codHabilitacao, byte setor)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_142);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 142;
            //--------------

            buf[i++] = (byte)(codHabilitacao >> 8);
            buf[i++] = (byte)codHabilitacao;
            buf[i++] = (byte)setor;

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_142);
            return buf;
        }

        //--------------------------------------------------------------------------------------------------------
        // 143	Editar habilitação das leitoras 2 (c/ SETOR)
        // CMD: 00 + 8F + <cód. da habilitação H> + <cód. da habilitação L> + <frame de habilitação (256 bytes)> + <setor> + cs   [262 bytes]
        // RSP: 00 + 8F + <retorno> + <cód. da habilitação H> + <cód. da habilitação L> + <setor> + cs   [7 bytes]
        public byte[] editarHabilitacaoLeitorasComSetor(int codHabilitacao, byte[] bufHabilitacao, byte setor)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_143);
            int i = 5, j = 0;

            buf[i++] = 0;
            buf[i++] = 143;
            //----------------------------------------------
            buf[i++] = (byte)(codHabilitacao >> 8); // high
            buf[i++] = (byte)codHabilitacao; // low
            for (j = 0; j < SIZE_OF_HABILITACAO; j++) buf[i++] = bufHabilitacao[j];
            buf[i++] = (byte)setor; // low
            //----------------------------------------------
            buf[i] = checksum(buf, N_BYTES_CMD_143);
            return buf;
        }
        //--------------------------------------------------------------------------------------------------------
        // 144	Ler / Gravar pacote de habilitação das leitoras 2 (c/ SETOR) (PACOTE DE DADOS)	
        // CMD: 00 + 90 + <operacao> + 00 + <indicePacote_L(de 0 a 25)> + <pacote(512 bytes)> + <setor> + cs          			  [519 bytes]
        // RSP: 00 + 90 + <retorno> + <operacao> + 00 + <indice pacote (de 0 a 25)> + <pacote(512 bytes)> + <setor> + cs          [520 bytes] 
        public byte[] operacaoPacoteHabilitacaoComSetor(byte operacao, UInt16 indice, byte[] pacoteDados, byte setor)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_144);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 144;
            //--------------------
            buf[i++] = (byte)operacao;
            buf[i++] = (byte)(indice >> 8);
            buf[i++] = (byte)indice;
            for (j = 0; j < 512; j++)
            {
                if (operacao == OP_ESCRITA)
                {
                    buf[i++] = pacoteDados[j];
                }
                else
                {
                    buf[i++] = 0;
                }
            }
            buf[i++] = (byte)setor;
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_144);
            return buf;
        }
        //--------------------------------------------------------------------------------------------------------
        // 145	Ler Grupo da leitora 2 (c/ SETOR) (GL)											
        // CMD: 00 + 91+ <cód. da habilitação H> + <cód. da habilitação L> + <endCan> + <setor> + cs         [7 bytes]									
        // RSP: 00 + 91 + <retorno> + <cód. da habilitação H> + <cód. da habilitação L> + <endCan> + <código da Jornada (leitora 1)> + <código da Jornada (leitora 2)> + <código da Jornada (leitora 3)> + <código da Jornada (leitora 4)> + <setor> + cs       [12 bytes]
        public byte[] lerGrupoLeitorasComSetor(int codLeitoras, byte enderecoPlaca, byte setor)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_145);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 145;
            //--------------

            buf[i++] = (byte)(codLeitoras >> 8);
            buf[i++] = (byte)codLeitoras;
            buf[i++] = enderecoPlaca;
            buf[i++] = setor;

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_145);
            return buf;
        }

        //--------------------------------------------------------------------------------------------------------
        // 146	Editar Grupo da leitora 2 (c/ SETOR) (GL)										
        // CMD: 00 + 92 + <cód. da habilitação H> +  <cód. da habilitação L> + <endCan> + <código da Jornada (leitora 1)> + <código da Jornada (leitora 2)> + <código da Jornada (leitora 3)> + <código da Jornada (leitora 4)> + <setor> + cs         [11 bytes]	
        // RSP: 00 + 92 + <retorno> + <cód. da habilitação H> +  <cód. da habilitação L> + <setor> + cs       [7 bytes]
        public byte[] editarGrupoLeitorasComSetor(int codLeitoras, byte enderecoPlaca, byte codigoGrupoLeitora1, byte codigoGrupoLeitora2, byte codigoGrupoLeitora3, byte codigoGrupoLeitora4, byte setor)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_146);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 146;
            //--------------

            buf[i++] = (byte)(codLeitoras >> 8);
            buf[i++] = (byte)codLeitoras;
            buf[i++] = enderecoPlaca;
            buf[i++] = codigoGrupoLeitora1;
            buf[i++] = codigoGrupoLeitora2;
            buf[i++] = codigoGrupoLeitora3;
            buf[i++] = codigoGrupoLeitora4;
            buf[i++] = setor;
            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_146);
            return buf;
        }

        //--------------------------------------------------------------------------------------------------------
        // 147	Ler / Gravar pacote de grupos de  leitora  2 (c/ SETOR) (GL)  (PACOTE DE DADOS)	
        // CMD: 00 + 93 + <operacao> + 00 + <indicePacote_L (de 0 a 207)> + <pacote(512 bytes)> + <setor> + cs          [519 bytes]	
        // RSP: 00 + 93 + <retorno> + <operacao> + 00 + <indice pacote (de 0 a 207)> + <pacote(512 bytes)> + <setor> + cs          [520 bytes] 
        public byte[] operacaoPacoteGrupoLeitorasComSetor(byte operacao, UInt16 indice, byte[] pacoteDados, byte setor)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_147);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 147;
            //--------------------
            buf[i++] = (byte)operacao;
            buf[i++] = (byte)(indice >> 8);
            buf[i++] = (byte)indice;
            for (j = 0; j < 512; j++)
            {
                if (operacao == OP_ESCRITA)
                {
                    buf[i++] = pacoteDados[j];
                }
                else
                {
                    buf[i++] = 0;
                }
            }
            buf[i++] = (byte)setor;
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_147);
            return buf;
        }

        //--------------------------------------------------------------------------------------------------------
        // 148	Ler/Edita habilitação das Leitoras individual 2 (c/ SETOR)						
        // CMD: 00 + 94 + <operação> + <cód. da habilitação H> + <cód. da habilitação L> + <nDisp> + <habilitação> + <labelRota(8bytes)> + <setor> + cs     [10 bytes]	
        // RSP: 00 + 94 + <retorno> + <operação> + <cód. da habilitação H> + <cód. da habilitação L> + <nDisp> + <habilitação> + <labelRota(8bytes)> +  <setor> + cs     [11 bytes]
        public byte[] operacaoRotaIndividualComSetor(byte operacao, UInt16 codigoRota, byte enderecoPlaca, byte bHabilitacao, String labelRota, byte setor)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_148);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 148;
            //--------------

            buf[i++] = operacao;
            buf[i++] = (byte)(codigoRota >> 8);
            buf[i++] = (byte)codigoRota;
            buf[i++] = enderecoPlaca;
            buf[i++] = bHabilitacao;
            for (j = 0; j < 8; j++)
            {
                if (j < labelRota.Length)
                {
                    buf[i++] = (byte)labelRota[j];
                }
                else
                {
                    buf[i++] = (byte)' ';
                }
            }
            buf[i++] = setor;
            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_148);
            return buf;
        }


        //--------------------------------------------------------------------------------------------------------
        // 149 Ler / Gravar habilitação e grupo da leitora (c/ SETOR)
        // 00 + 95 + <operacao> + <cód. da rota H> + <cód. da rota L> + <nDisp> + <habilitação das 4 leitoras(bit3/bit2/bit1/bit0)> +
        //         <códGrupo(leitora1)> + <códGrupo(leitora2)> + <códGrupo(leitora3)> + <códGrupo(leitora4)> + <labelRota(8bytes)> + <setor> + cs      [21 bytes]
        // 00 + 95 + <retorno> + <operacao> + <cód. da rota H> + <cód. da rota L> + <nDisp> + <habilitação das 4 leitoras(bit3/bit2/bit1/bit0)> + 
        //         <códGrupo(leitora1)> + <códGrupo(leitora2)> + <códGrupo(leitora3)> + <códGrupo(leitora4)> + <labelRota(8bytes)> + <setor> + cs      [22 bytes]
        public byte[] operacaoRotaGrupoLeitorasComSetor(byte operacao, UInt16 codigoRota, byte enderecoPlaca, byte habilitacaoLeitoras,
                                                        byte codigoGrupoLeitora1, byte codigoGrupoLeitora2, byte codigoGrupoLeitora3, byte codigoGrupoLeitora4,
                                                        String labelRota, byte setor)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_149);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 149;
            //--------------------
            buf[i++] = operacao;
            buf[i++] = (byte)(codigoRota >> 8);
            buf[i++] = (byte)codigoRota;
            buf[i++] = enderecoPlaca;
            buf[i++] = habilitacaoLeitoras;
            buf[i++] = codigoGrupoLeitora1;
            buf[i++] = codigoGrupoLeitora2;
            buf[i++] = codigoGrupoLeitora3;
            buf[i++] = codigoGrupoLeitora4;
            for (j = 0; j < 8; j++)
            {
                if (j < labelRota.Length)
                {
                    buf[i++] = Convert.ToByte(labelRota[j]);
                }
                else
                {
                    buf[i++] = Convert.ToByte(' ');
                }
            }
            buf[i++] = setor;
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_149);
            return buf;
        }
        //--------------------------------------------------------------------------------------------------------
        // 150	Ler / Gravar pacote de Labels de rota  (PACOTE DE DADOS)
        // 00 + 96 + <operacao> + 00 + <indicePacote_L (de 0 a 1)> + <pacote(512 bytes)> + <setor> + cs          			[519 bytes]
        // 00 + 96 + <retorno> + <operacao> + 00 + <indice pacote (de 0 a 1)> + <pacote(512 bytes)> + <setor> + cs          [520 bytes]
        public byte[] operacaoPacoteLabelsRotaComSetor(byte operacao, UInt16 indice, byte[] pacoteDados, byte setor)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_150);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 150;
            //--------------------
            buf[i++] = (byte)operacao;
            buf[i++] = (byte)(indice >> 8);
            buf[i++] = (byte)indice;
            for (j = 0; j < 512; j++)
            {
                if (operacao == OP_ESCRITA)
                {
                    buf[i++] = pacoteDados[j];
                }
                else
                {
                    buf[i++] = 0;
                }
            }
            buf[i++] = (byte)setor;
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_150);
            return buf;
        }
        //--------------------------------------------------------------------------------------------------------
        // 151	Ler/Editar midia (sdcard)
        // CMD: 00 + 97 + <operação> + <midia> + <caminho(até 54 bytes - ASCII)> + <nBytesDadosH> + <nBytesDadosL> + <dados(bytesDados de 0 a 512)> + cs   [ 61 + nBytes ]
        // RSP: 00 + 97 + <retorno> + <operação> + <midia> + <caminho(até 54 bytes - ASCII)> + <nBytesDadosH> + <nBytesDadosL> + <dados(bytesDados de 0 a 512)> + cs   [ 62 + nBytes ]
        public byte[] lerEditarMidia(byte operacao, byte midia, byte[] path, UInt16 tamanho, byte[] dados)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_151 + tamanho);
            int i = 5, j = 0;

            buf[i++] = 0;
            buf[i++] = 151;
            //--------------------
            buf[i++] = operacao;
            buf[i++] = midia;
            for (j = 0; j < 54; j++) buf[i++] = path[j];
            buf[i++] = (byte)(tamanho >> 8);
            buf[i++] = (byte)tamanho;
            for (j = 0; j < tamanho; j++) buf[i++] = dados[j];

            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_151 + tamanho);
            return buf;
        }
        //--------------------------------------------------------------------------------------------------------
        // 152	Ler/Editar Controle de Vagas ID	
        // CMD: 00 + 98 + <operação> + <indice_id_L> + indice_id_H> + <id(6 bytes)> + <qtd_total_vagas> + <qtd_atual_vagas> + cs   [14 bytes] (FINALIZAR COM 0xFF no campo "id")	
        // RSP: 00 + 98 + <retorno> + <operação> + <indice_id_H> + indice_id_L> + <id(6 bytes)> + <qtd_total_vagas> + <qtd_atual_vagas> + cs  [15 bytes]
        public byte[] operacaoControleVagasID(byte operacao, UInt16 indice, String id, byte qtdTotaVagas, byte qtdAtualVagas)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_152);
            int i = 5;

            buf[i++] = 0;
            buf[i++] = 152;
            //--------------------
            buf[i++] = operacao;
            buf[i++] = (byte)(indice >> 8);
            buf[i++] = (byte)(indice);
            for (int j = 0; j < 6; j++)
            {
                if (j < id.Length)
                {
                    buf[i++] = (byte)id[j];
                }
                else
                {
                    buf[i++] = (byte)' ';
                }
            }
            buf[i++] = qtdTotaVagas;
            buf[i++] = qtdAtualVagas;

            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_152);
            return buf;
        }
        //--------------------------------------------------------------------------------------------------------
        // 153	Ler/Editar Controle de Vagas ID (PACOTE DE DADOS)	        
        // CMD: 00 + 99 + <operação> + <indicePacote_H> + <indicePacote_L> + <pacote(512 bytes)> + cs             [518 bytes]	
        // RSP: 00 + 99 + <retorno> + <operação> + <indicePacote_H> + <indicePacote_L> + <pacote(512 bytes)> + cs             [519 bytes]
        public byte[] operacaoPacoteControleVagasId(byte operacao, UInt16 indice, byte[] pacoteDados)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_153);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 153;
            //--------------------
            buf[i++] = (byte)operacao;
            buf[i++] = (byte)(indice >> 8);
            buf[i++] = (byte)indice;
            for (j = 0; j < 512; j++)
            {
                if (operacao == OP_ESCRITA)
                {
                    buf[i++] = pacoteDados[j];
                }
                else
                {
                    buf[i++] = 0;
                }
            }
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_153);
            return buf;
        }
        //--------------------------------------------------------------------------------------------------------
        // 154	Inicializar Vagas ID
        // CMD: 00 + 9A + <indice_id_H> + indice_id_L> + <RecarregarTotalVagas(0=AtualVagas = 0 / 1=AtualVagas = TotalVagas)> + cs 	[6 bytes]
        // CMD: 00 + 9A + <retorno> + <indice_id_H> + indice_id_L> + <RecarregarComTotalVagas> + cs  [7 bytes]
        public byte[] inicializarVagasId(UInt16 indice, Boolean recarregarTotalVagas)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_154);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 154;
            //--------------------
            buf[i++] = (byte)(indice >> 8);
            buf[i++] = (byte)indice;
            if (recarregarTotalVagas)
                buf[i++] = 1;
            else
                buf[i++] = 0;
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_154);
            return buf;
        }
        //--------------------------------------------------------------------------------------------------------
        // 155	Cancelar Panico
        // CMD: 00 + 9B + <saída digital> + cs 	[4 bytes]
        // CMD: 00 + 9B + <retorno> + <saída digital> + cs     [5 bytes]
        public byte[] cancelarPanico(byte saida)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_155);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 155;
            //--------------------
            buf[i++] = saida;
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_155);
            return buf;
        }

        //--------------------------------------------------------------
        // 156	Gravar / Editar - Biometria / Identificar Template
        // CMD: 00 + 9C + <operacao>+  <indiceLeitora> + <tipoBiometria> + <frame dispositivo(32 bytes)> + <tamanhoTemplateH> + <tamanhoTemplateL> + <template> + cs         [40 bytes + template]
        // RSP: 00 + 9C + <retorno> + <operacao> + <indiceLeitora> + <serial 4> + <serial 5> + cs       [8 bytes]
        public byte[] operacoesBiometria3(byte operacao, byte indiceLeitora, byte tipoBiometria, byte[] frameDispositivo, UInt16 tamanhoTemplate, byte[] template)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_156 + tamanhoTemplate);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 156;
            //--------------

            buf[i++] = operacao;
            buf[i++] = indiceLeitora;
            buf[i++] = tipoBiometria;
            for (j = 0; j < SIZE_OF_DISPOSITIVO; j++) buf[i++] = frameDispositivo[j];
            buf[i++] = (byte)(tamanhoTemplate >> 8);
            buf[i++] = (byte)tamanhoTemplate;
            for (j = 0; j < tamanhoTemplate; j++) buf[i++] = template[j];

            //--------------
            buf[i] = checksum(buf, N_BYTES_CMD_156 + tamanhoTemplate);
            return buf;
        }

        //--------------------------------------------------------------
        // 157	Editar turnos 2
        // CMD: 00 + 9D + <cód. do turno> + <frame de turno(16 bytes)> + <direcaoTurnos> + cs         [21 bytes]
        // RSP: 00 + 9D + <retorno>  + <cód. do turno> + cs       [5 bytes]
        public byte[] editarTurnos2(int codTurno, byte[] turnos, byte direcaoTurnos)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_157);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 157;
            //--------------------
            buf[i++] = (byte)codTurno;
            for (j = 0; j < 16; j++) buf[i++] = turnos[j];
            buf[i++] = direcaoTurnos;
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_157);
            return buf;
        }

        //--------------------------------------------------------------
        // 158	Ler turnos 2	00 + 59 + <cód. do turno> + cs         [4 bytes]
        // CMD: 00 + 9E + <cód. do turno> + cs         [4 bytes]
        // RSP: 00 + 9E + <retorno> + <cód. do turno> + <frame de turno(16 bytes)> + <direcaoTurnos> + cs       [22 bytes]
        public byte[] lerTurnos2(int codTurnos)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_158);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 158;
            //--------------------
            buf[i++] = (byte)codTurnos;
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_158);
            return buf;
        }


        //--------------------------------------------------------------
        // 159	Ler / Gravar pacote de turnos (PACOTE DE DADOS)
        // CMD: 00 + 9F + <operacao> + 00 + <indice pacote turno (de 0 a 3)> + <pacote(512 bytes)> + cs   [518 bytes]
        // RSP: 00 + 9F + <retorno>  + <operacao> + 00 + <pacote(512 bytes)> + cs       [519 bytes] 
        //       
        public byte[] operacaoPacoteTurnos2(byte operacao, UInt16 indice, byte[] pacoteDados)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_159);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 0x9F;
            //--------------------
            buf[i++] = (byte)operacao;
            buf[i++] = (byte)(indice >> 8);
            buf[i++] = (byte)indice;
            for (j = 0; j < 512; j++)
            {
                if (operacao == OP_ESCRITA)
                {
                    buf[i++] = pacoteDados[j];
                }
                else
                {
                    buf[i++] = 0;
                }
            }
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_159);
            return buf;
        }

        //--------------------------------------------------------------
        // 160	Restore de biometrias SDCARD
        // CMD: 00 + A0 + cs	            [3 bytes]			
        // RSP: 00 + A0 + <retorno> + cs 	[4 bytes]
        //       
        public byte[] restoreBiomtriasSdcard(byte[] filename)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_160);
            int i = 5, j;
            buf[i++] = 0;
            buf[i++] = 0xA0;
            //--------------------   
            for (j = 0; j < 13; j++) buf[i++] = filename[j];
            //--------------------
            buf[i] = checksum(buf, N_BYTES_CMD_160);
            return buf;
        }

        //--------------------------------------------------------------
        // 161	Força atualização automática de creditos no modo de contagem.
        // CMD: 00 + A1 + cs                    [3 bytes]
        // RSP: 00 + A1 + <retorno> + cs        [4 bytes]
        //       
        public byte[] atualizaCreditoContagem()
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_161);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 0xA1;
            //--------------------------------------
            buf[i] = checksum(buf, N_BYTES_CMD_161);
            return buf;
        }

        //--------------------------------------------------------------
        // 162	Ler/ Gravar ajuste do relógio
        // CMD: 00 + A2 + <operação> + <offset> + cs					[5 bytes]
        // RSP: 00 + A2 + <retorno> + <operação> + <offset> + cs		[6 bytes]
        public byte[] operacaoAjusteRelogio(byte operacao, byte ajuste)
        {
            byte[] buf = criaBufferCmd(N_BYTES_CMD_162);
            int i = 5;
            buf[i++] = 0;
            buf[i++] = 0xA2;
            buf[i++] = (byte)operacao;
            //--------------------------------------
            buf[i++] = ajuste;
            //--------------------------------------
            buf[i] = checksum(buf, N_BYTES_CMD_162);
            return buf;
        }


        //--------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------
        public byte[] setupParaVetor(s_setup setup)
        {
            byte[] v = new byte[SIZE_OF_SETUP];
            UInt16 i = 0, j;

            v[i++] = setup.cntAtual;
            v[i++] = setup.endCan;
            v[i++] = setup.modo;
            v[i++] = (byte)((setup.cfg1.giroCatraca << 5) + (setup.cfg1.RFHabilitado << 4) + (setup.cfg1.serialMestre << 3) + (setup.cfg1.ctrlVagasNivel << 2) + setup.cfg1.brCan);
            v[i++] = (byte)((setup.cfg2.funcaoUart3 << 5) + (setup.cfg2.funcaoUart2 << 2) + (setup.cfg2.umaSolenoide << 1) + setup.cfg2.sentidoCatraca);
            v[i++] = (byte)((setup.cfg3.enviarTemplateNaoCadastrado << 7) + (setup.cfg3.baudRateUart2 << 5) + (setup.cfg3.stopBitsUart1 << 4) + (setup.cfg3.paridadeUart1 << 2) + setup.cfg3.baudRateUart1);
            v[i++] = (byte)((setup.cfg4.tipoDispLeitora2 << 5) + (setup.cfg4.tipoDispLeitora1 << 2) + setup.cfg4.baudRateUart3);
            v[i++] = (byte)((setup.cfg5.sinalizacaoSaidasDigitais << 6) + (setup.cfg5.tipoDispLeitora4 << 3) + setup.cfg5.tipoDispLeitora3);
            v[i++] = (byte)((setup.cfg6.eventoSensor4 << 6) + (setup.cfg6.eventoSensor3 << 4) + (setup.cfg6.eventoSensor2 << 2) + setup.cfg6.eventoSensor1);
            v[i++] = setup.reservado_9;
            v[i++] = setup.reservado_10;
            v[i++] = setup.nivel;
            v[i++] = (byte)(setup.vagas >> 8);
            v[i++] = (byte)setup.vagas;
            for (j = 0; j < N_LEITORAS; j++) v[i++] = setup.tRele[j];
            for (j = 0; j < N_LEITORAS; j++) v[i++] = setup.tSaida[j];
            v[i++] = setup.tempoPassagem;
            v[i++] = setup.tempoLeituraCofre;
            for (j = 0; j < N_LEITORAS; j++) v[i++] = setup.toutPassagem[j];
            for (j = 0; j < N_LEITORAS; j++) v[i++] = setup.anticarona[j];
            v[i++] = setup.cntAtualDisp;
            v[i++] = setup.setor;
            for (j = 0; j < N_LEITORAS; j++) v[i++] = setup.atrasoEntradaLeitora[j];

            for (j = 0; j < N_LEITORAS; j++) v[i++] = setup.parametro_01_Leitora[j];
            for (j = 0; j < N_LEITORAS; j++) v[i++] = setup.tLeitoraRS485_10ms[j];
            for (j = 0; j < N_LEITORAS; j++) v[i++] = setup.funcaoRS485[j];

            for (j = 0; j < SIZE_OF_BYTES_RESERVADOS; j++) v[i++] = setup.reservado[j];
            v[i++] = (byte)(setup.ip >> 24);
            v[i++] = (byte)(setup.ip >> 16);
            v[i++] = (byte)(setup.ip >> 8);
            v[i++] = (byte)setup.ip;
            v[i++] = (byte)(setup.mask >> 24);
            v[i++] = (byte)(setup.mask >> 16);
            v[i++] = (byte)(setup.mask >> 8);
            v[i++] = (byte)setup.mask;
            v[i++] = (byte)(setup.gw >> 24);
            v[i++] = (byte)(setup.gw >> 16);
            v[i++] = (byte)(setup.gw >> 8);
            v[i++] = (byte)setup.gw;
            v[i++] = (byte)(setup.ipDestino >> 24);
            v[i++] = (byte)(setup.ipDestino >> 16);
            v[i++] = (byte)(setup.ipDestino >> 8);
            v[i++] = (byte)setup.ipDestino;
            v[i++] = (byte)((setup.cfge.DDNShabilitado << 3) + (setup.cfge.dhcp << 2) + setup.cfge.DDNS);
            v[i++] = setup.toutEthernet1;
            v[i++] = setup.toutEthernet2;
            v[i++] = (byte)(setup.porta1 >> 8);
            v[i++] = (byte)setup.porta1;
            v[i++] = (byte)(setup.porta2 >> 8);
            v[i++] = (byte)setup.porta2;
            v[i++] = (byte)(setup.porta3 >> 8);
            v[i++] = (byte)setup.porta3;
            v[i++] = (byte)(setup.porta4 >> 8);
            v[i++] = (byte)setup.porta4;
            for (j = 0; j < SIZE_OF_DDNS_USUARIO; j++) v[i++] = setup.DDNSusuario[j];
            for (j = 0; j < SIZE_OF_DDNS_SENHA; j++) v[i++] = setup.DDNSsenha[j];
            for (j = 0; j < SIZE_OF_DDNS_DEVICE; j++) v[i++] = setup.DDNSdevice[j];
            for (j = 0; j < SIZE_OF_USUARIO_LOGIN; j++) v[i++] = setup.usuarioLogin[j];
            for (j = 0; j < SIZE_OF_SENHA_LOGIN; j++) v[i++] = setup.senhaLogin[j];
            for (j = 0; j < SIZE_OF_DNS_HOST; j++) v[i++] = setup.dnsHost[j];
            for (j = 0; j < SIZE_OF_DDNS_HOST; j++) v[i++] = setup.DDNShost[j];
            v[i++] = setup.toleranciaHorario;
            for (j = 0; j < N_LEITORAS; j++) v[i++] = setup.funcaoLeitora[j];
            v[i++] = setup.tempoLedBuzzerLeitoras;
            v[i++] = (byte)((setup.cfg7.toutMsgDisplay << 4) + (setup.cfg7.avisoLowBat << 3) + setup.cfg7.toutPanico);
            v[i++] = setup.toutPassback;
            v[i++] = (byte)((setup.cfg8.emHorarioVerao << 7) + (setup.cfg8.horarioVeraoHabilitado << 6) + (setup.cfg8.autonomia << 4) + (setup.cfg8.leitoraManchester << 3) + (setup.cfg8.saidaLivre << 2) + setup.cfg8.catraca2010);
            for (j = 0; j < 4; j++) v[i++] = setup.horarioVerao[j];
            v[i++] = (byte)(setup.porta5 >> 8);
            v[i++] = (byte)setup.porta5;
            v[i++] = (byte)(setup.porta6 >> 8);
            v[i++] = (byte)setup.porta6;
            v[i++] = (byte)((setup.cfg9.umSensorParaDuasLeitoras_3e4 << 7) + (setup.cfg9.eventoEntradaDigitalAberta << 6) + (setup.cfg9.alarmeEntradaDigitalAberta << 5) + (setup.cfg9.considerarWiegand26bits << 4) + (setup.cfg9.umSensorParaDuasLeitoras_1e2 << 3) + (setup.cfg9.antipassbackDesligado << 2) + (setup.cfg9.tratarBiometria_1_1 << 1) + (setup.cfg9.catracaAdaptada));
            v[i++] = (byte)(setup.porta7 >> 8);
            v[i++] = (byte)setup.porta7;
            v[i++] = setup.tempoEventoEntradaDigitalAberta;

            v[i++] = (byte)((setup.cfg10.filtrarEvento7 << 7)
                              + (setup.cfg10.filtrarEvento6 << 6)
                              + (setup.cfg10.filtrarEvento5 << 5)
                              + (setup.cfg10.filtrarEvento4 << 4)
                              + (setup.cfg10.filtrarEvento3 << 3)
                              + (setup.cfg10.filtrarEvento2 << 2)
                              + (setup.cfg10.filtrarEvento1 << 1)
                              + (setup.cfg10.filtrarEvento0));
            v[i++] = (byte)((setup.cfg11.filtrarEvento15 << 7)
                              + (setup.cfg11.filtrarEvento14 << 6)
                              + (setup.cfg11.filtrarEvento13 << 5)
                              + (setup.cfg11.filtrarEvento12 << 4)
                              + (setup.cfg11.filtrarEvento11 << 3)
                              + (setup.cfg11.filtrarEvento10 << 2)
                              + (setup.cfg11.filtrarEvento9 << 1)
                              + (setup.cfg11.filtrarEvento8));
            v[i++] = (byte)((setup.cfg12.filtrarEvento23 << 7)
                              + (setup.cfg12.filtrarEvento22 << 6)
                              + (setup.cfg12.filtrarEvento21 << 5)
                              + (setup.cfg12.filtrarEvento20 << 4)
                              + (setup.cfg12.filtrarEvento19 << 3)
                              + (setup.cfg12.filtrarEvento18 << 2)
                              + (setup.cfg12.filtrarEvento17 << 1)
                              + (setup.cfg12.filtrarEvento16));
            v[i++] = setup.toutAlarmeEntradaDigitalAberta;
            v[i++] = (byte)((setup.cfg13.controleVagasRota << 6) + (setup.cfg13.validarBiometriaOnline_emRemoto << 5) + (setup.cfg13.enviarEventoIndexado << 4) + (setup.cfg13.desvincularVisitante << 3) + (setup.cfg13.nivelSegurancaBiometria));
            v[i++] = setup.toutAcionamentoLedBuzzer;
            v[i++] = (byte)(setup.portaHttp >> 8);
            v[i++] = (byte)(setup.portaHttp);

            v[i++] = (byte)((setup.cfg14.panicoSaidaDigital)
                             + (setup.cfg14.cartao2xPanico << 2)
                             + (setup.cfg14.tipoTag_UHF << 3)
                             + (setup.cfg14.varreSimultanea_UHF << 5)
                             + (setup.cfg14.wiegand34_UHF << 6)
                             + (setup.cfg14.frequenciaHopping_UHF << 7));

            v[i++] = (byte)((setup.cfg15.buzzer_L1_UHF_on)
                             + (setup.cfg15.buzzer_L2_UHF_on << 1)
                             + (setup.cfg15.buzzer_L3_UHF_on << 2)
                             + (setup.cfg15.buzzer_L4_UHF_on << 3)
                             + (setup.cfg15.rele5_eventoClonagem << 4)
                             + (setup.cfg15.rele5_eventoPanico << 5)
                             + (setup.cfg15.rele5_eventoAlarme << 6)
                             + (setup.cfg15.rele5_eventoLigado << 7));

            v[i++] = setup.tRele5;

            v[i++] = (byte)((setup.cfg16.senha13digitos)
                             + (setup.cfg16.tipoCatraca << 1)
                             + (setup.cfg16.direcaoTurnos << 5)
                             + (setup.cfg16.panicoSenha99 << 6)
                             + (setup.cfg16.desligarBuzzer << 7));

            v[i++] = (byte)(setup.porta8 >> 8);
            v[i++] = (byte)setup.porta8;

            v[i++] = (byte)(setup.porta9 >> 8);
            v[i++] = (byte)setup.porta9;

            v[i++] = (byte)(setup.porta10 >> 8);
            v[i++] = (byte)setup.porta10;

            v[i++] = (byte)(setup.porta11 >> 8);
            v[i++] = (byte)setup.porta11;

            v[i++] = (byte)(setup.porta12 >> 8);
            v[i++] = (byte)setup.porta12;

            v[i++] = (byte)(setup.porta13 >> 8);
            v[i++] = (byte)setup.porta13;

            v[i++] = (byte)((setup.cfg17.baudrateRS485_1)
                             + (setup.cfg17.baudrateRS485_2 << 2)
                             + (setup.cfg17.baudrateRS485_3 << 4)
                             + (setup.cfg17.baudrateRS485_4 << 6));

            v[i++] = (byte)((setup.cfg18.retardoDesligarSolenoide & 0x03)
                          + (setup.cfg18.desligaRetornoAutoT5S << 2)
                          + (setup.cfg18.facilityWieg66 << 3)
                          + (setup.cfg18.habilitaContagemPassagem << 4)
                          + (setup.cfg18.habilitaAtualizacaoAuto << 5)
                          + (setup.cfg18.habilitaIDMestre << 6)
                          + (setup.cfg18.habilitaAcomodacao << 7));

            v[i++] = (byte)(setup.horarioAtualizaAuto[0]);
            v[i++] = (byte)(setup.horarioAtualizaAuto[1]);

            v[i++] = (byte)((setup.cfg19.sensorPassagemCofre)
                          + (setup.cfg19.sw_watchdog << 1)
                          + (setup.cfg19.verificarFirmwareFTP << 2)
                          + (setup.cfg19.sensorGiroInvertido << 3)
                          + (setup.cfg19.desativaRelePassagem << 4)
                          + (setup.cfg19.antipassbackDesligado_entrada << 5)
                          + (setup.cfg19.antipassbackDesligado_saida << 6)
                          + (setup.cfg19.duplaValidacaoVisitante << 7));

            v[i++] = (byte)setup.TempoQuedaCartao;

            v[i++] = setup.tRele6;

            return v;
        }


        //--------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------
        public s_setup vetorParaSetup(byte[] buf)
        {
            controladoraLinear.FLAGS_CFG_E flagsE = new controladoraLinear.FLAGS_CFG_E();
            controladoraLinear.FLAGS_CFG_1 flags1 = new controladoraLinear.FLAGS_CFG_1(0, 0, 0, 0, 0);
            controladoraLinear.FLAGS_CFG_2 flags2 = new controladoraLinear.FLAGS_CFG_2(0, 0, 0, 0);
            controladoraLinear.FLAGS_CFG_3 flags3 = new controladoraLinear.FLAGS_CFG_3(0, 0, 0, 0, 0);
            controladoraLinear.FLAGS_CFG_4 flags4 = new controladoraLinear.FLAGS_CFG_4(0, 0, 0);
            controladoraLinear.FLAGS_CFG_5 flags5 = new controladoraLinear.FLAGS_CFG_5(0, 0, 0);
            controladoraLinear.FLAGS_CFG_6 flags6 = new controladoraLinear.FLAGS_CFG_6(0, 0, 0, 0);
            controladoraLinear.FLAGS_CFG_7 flags7 = new controladoraLinear.FLAGS_CFG_7(0, 0, 0);
            controladoraLinear.FLAGS_CFG_8 flags8 = new controladoraLinear.FLAGS_CFG_8(0, 0, 0, 0, 0, 0);
            controladoraLinear.FLAGS_CFG_9 flags9 = new controladoraLinear.FLAGS_CFG_9(0, 0, 0, 0, 0, 0, 0, 0);
            controladoraLinear.FLAGS_CFG_10 flags10 = new controladoraLinear.FLAGS_CFG_10(0, 0, 0, 0, 0, 0, 0, 0);
            controladoraLinear.FLAGS_CFG_11 flags11 = new controladoraLinear.FLAGS_CFG_11(0, 0, 0, 0, 0, 0, 0, 0);
            controladoraLinear.FLAGS_CFG_12 flags12 = new controladoraLinear.FLAGS_CFG_12(0, 0, 0, 0, 0, 0, 0, 0);
            controladoraLinear.FLAGS_CFG_13 flags13 = new controladoraLinear.FLAGS_CFG_13(0, 0, 0, 0, 0, 0);
            controladoraLinear.FLAGS_CFG_14 flags14 = new controladoraLinear.FLAGS_CFG_14(0, 0, 0, 0, 0, 0);
            controladoraLinear.FLAGS_CFG_15 flags15 = new controladoraLinear.FLAGS_CFG_15(0, 0, 0, 0, 0, 0, 0, 0);
            controladoraLinear.FLAGS_CFG_16 flags16 = new controladoraLinear.FLAGS_CFG_16(0, 0, 0, 0, 0);
            controladoraLinear.FLAGS_CFG_17 flags17 = new controladoraLinear.FLAGS_CFG_17(0, 0, 0, 0);
            controladoraLinear.FLAGS_CFG_18 flags18 = new controladoraLinear.FLAGS_CFG_18(0, 0, 0, 0, 0, 0, 0);
            controladoraLinear.FLAGS_CFG_19 flags19 = new controladoraLinear.FLAGS_CFG_19(0, 0, 0, 0, 0, 0, 0, 0);

            byte[,] dispositivos = new byte[controladoraLinear.N_LINES_DISPOSITIVO, controladoraLinear.SIZE_OF_DISPOSITIVO];
            byte[,] rotas = new byte[controladoraLinear.N_LINES_HABILITACAO, controladoraLinear.SIZE_OF_HABILITACAO];
            byte[,] gruposLeitoras = new byte[controladoraLinear.N_LINES_GRUPO_LEITORA, controladoraLinear.SIZE_OF_GRUPO_LEITORA];
            byte[,] jornadas = new byte[controladoraLinear.N_LINES_JORNADAS, controladoraLinear.SIZE_OF_JORNADAS];
            byte[,] turnos = new byte[controladoraLinear.N_LINES_TURNOS, controladoraLinear.SIZE_OF_TURNOS];
            byte[] feriados = new byte[controladoraLinear.N_LINES_FERIADO * controladoraLinear.SIZE_OF_FERIADO];
            byte[,] labelsRotas = new byte[controladoraLinear.N_LINES_HABILITACAO, 8];
            byte[,] mensagensDisplay = new byte[12, 32];

            s_setup setup = new controladoraLinear.s_setup(0, 0, 0, 0, 0, 0, 0, new byte[controladoraLinear.N_LEITORAS], new byte[controladoraLinear.N_LEITORAS], 0,
                             0, new byte[controladoraLinear.N_LEITORAS], new byte[controladoraLinear.N_LEITORAS], 0, 0, new byte[controladoraLinear.N_LEITORAS], new byte[controladoraLinear.N_LEITORAS], new byte[controladoraLinear.N_LEITORAS], new byte[controladoraLinear.N_LEITORAS], new byte[controladoraLinear.SIZE_OF_BYTES_RESERVADOS], 0, 0, 0, 0,
                             0, 0, 0, 0, 0, 0, new byte[controladoraLinear.SIZE_OF_DDNS_USUARIO], new byte[controladoraLinear.SIZE_OF_DDNS_SENHA], new byte[controladoraLinear.SIZE_OF_DDNS_DEVICE],
                             new byte[controladoraLinear.SIZE_OF_USUARIO_LOGIN], new byte[controladoraLinear.SIZE_OF_SENHA_LOGIN], new byte[controladoraLinear.SIZE_OF_DNS_HOST], new byte[controladoraLinear.SIZE_OF_DDNS_HOST], 0, new byte[controladoraLinear.N_LEITORAS], 0,
                             0, new byte[4], 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 80, 0, 0,
                             flagsE, flags1, flags2, flags3, flags4, flags5, flags6, flags7, flags8, flags9, flags10, flags11, flags12, flags13, flags14, flags15, flags16, flags17, flags18, new byte[2], flags19, 0);

            //-----------------------------------------------

            UInt16 i = 3, j;

            setup.cntAtual = buf[i++];
            setup.endCan = buf[i++];
            setup.modo = buf[i++];

            //buf[i++] = (byte)((setup.cfg1.giroCatraca << 5) + (setup.cfg1.RFHabilitado << 4) + (setup.cfg1.serialMestre << 3) + (setup.cfg1.ctrlVagasNivel << 2) + setup.cfg1.brCan);
            setup.cfg1.brCan = (byte)(buf[i] & 0x07);
            setup.cfg1.ctrlVagasNivel = (byte)((buf[i] & 0x04) >> 2);
            setup.cfg1.serialMestre = (byte)((buf[i] & 0x08) >> 3);
            setup.cfg1.RFHabilitado = (byte)((buf[i] & 0x10) >> 4);
            setup.cfg1.giroCatraca = (byte)((buf[i] & 0xE0) >> 5);
            i++;

            //buf[i++] = (byte)((setup.cfg2.funcaoUart3 << 5) + (setup.cfg2.funcaoUart2 << 2) + (setup.cfg2.umaSolenoide << 1) + setup.cfg2.sentidoCatraca);
            setup.cfg2.sentidoCatraca = (byte)(buf[i] & 0x01);
            setup.cfg2.umaSolenoide = (byte)((buf[i] & 0x02) >> 1);
            setup.cfg2.funcaoUart2 = (byte)((buf[i] & 0x1c) >> 2);
            setup.cfg2.funcaoUart3 = (byte)((buf[i] & 0xe0) >> 5);
            i++;

            //buf[i++] = (byte)((setup.cfg3.enviarTemplateNaoCadastrado << 7) + (setup.cfg3.baudRateUart2 << 5) + (setup.cfg3.stopBitsUart1 << 4) + (setup.cfg3.paridadeUart1 << 2) + setup.cfg3.baudRateUart1);
            setup.cfg3.baudRateUart1 = (byte)(buf[i] & 0x03);
            setup.cfg3.paridadeUart1 = (byte)((buf[i] & 0x0c) >> 2);
            setup.cfg3.stopBitsUart1 = (byte)((buf[i] & 0x10) >> 4);
            setup.cfg3.baudRateUart2 = (byte)((buf[i] & 0x60) >> 5);
            setup.cfg3.enviarTemplateNaoCadastrado = (byte)((buf[i] & 0x80) >> 7);
            i++;

            //buf[i++] = (byte)((setup.cfg4.tipoDispLeitora2 << 5) + (setup.cfg4.tipoDispLeitora1 << 2) + setup.cfg4.baudRateUart3);
            setup.cfg4.baudRateUart3 = (byte)(buf[i] & 0x03);
            setup.cfg4.tipoDispLeitora1 = (byte)((buf[i] & 0x1c) >> 2);
            setup.cfg4.tipoDispLeitora2 = (byte)((buf[i] & 0xe0) >> 5);
            i++;

            //buf[i++] = (byte)((setup.cfg5.sinalizacaoSaidasDigitais << 6) + (setup.cfg5.tipoDispLeitora4 << 3) + setup.cfg5.tipoDispLeitora3);
            setup.cfg5.tipoDispLeitora3 = (byte)(buf[i] & 0x07);
            setup.cfg5.tipoDispLeitora4 = (byte)((buf[i] & 0x38) >> 3);
            setup.cfg5.sinalizacaoSaidasDigitais = (byte)((buf[i] & 0xc0) >> 6);
            i++;

            //buf[i++] = (byte)((setup.cfg6.eventoSensor4 << 6) + (setup.cfg6.eventoSensor3 << 4) + (setup.cfg6.eventoSensor2 << 2) + setup.cfg6.eventoSensor1);
            setup.cfg6.eventoSensor1 = (byte)(buf[i] & 0x03);
            setup.cfg6.eventoSensor2 = (byte)((buf[i] & 0x0c) >> 2);
            setup.cfg6.eventoSensor3 = (byte)((buf[i] & 0x30) >> 4);
            setup.cfg6.eventoSensor4 = (byte)((buf[i] & 0xc0) >> 6);
            i++;

            setup.reservado_9 = buf[i++];
            setup.reservado_10 = buf[i++];
            setup.nivel = buf[i++];

            setup.vagas = (UInt16)(buf[i++] << 8);
            setup.vagas += (UInt16)buf[i++];

            for (j = 0; j < N_LEITORAS; j++) setup.tRele[j] = buf[i++];
            for (j = 0; j < N_LEITORAS; j++) setup.tSaida[j] = buf[i++];

            setup.tempoPassagem = buf[i++];
            setup.tempoLeituraCofre = buf[i++];

            for (j = 0; j < N_LEITORAS; j++) setup.toutPassagem[j] = buf[i++];
            for (j = 0; j < N_LEITORAS; j++) setup.anticarona[j] = buf[i++];
            setup.cntAtualDisp = buf[i++];

            setup.setor = (byte)(buf[i++] & 0x07);
            for (j = 0; j < N_LEITORAS; j++) setup.atrasoEntradaLeitora[j] = buf[i++];

            for (j = 0; j < N_LEITORAS; j++) setup.parametro_01_Leitora[j] = buf[i++];
            for (j = 0; j < N_LEITORAS; j++) setup.tLeitoraRS485_10ms[j] = buf[i++];
            for (j = 0; j < N_LEITORAS; j++) setup.funcaoRS485[j] = buf[i++];

            for (j = 0; j < SIZE_OF_BYTES_RESERVADOS; j++) setup.reservado[j] = buf[i++];

            setup.ip = (UInt32)(buf[i++] << 24);
            setup.ip += (UInt32)(buf[i++] << 16);
            setup.ip += (UInt32)(buf[i++] << 8);
            setup.ip += (UInt32)(buf[i++]);

            setup.mask = (UInt32)(buf[i++] << 24);
            setup.mask += (UInt32)(buf[i++] << 16);
            setup.mask += (UInt32)(buf[i++] << 8);
            setup.mask += (UInt32)(buf[i++]);

            setup.gw = (UInt32)(buf[i++] << 24);
            setup.gw += (UInt32)(buf[i++] << 16);
            setup.gw += (UInt32)(buf[i++] << 8);
            setup.gw += (UInt32)(buf[i++]);

            setup.ipDestino = (UInt32)(buf[i++] << 24);
            setup.ipDestino += (UInt32)(buf[i++] << 16);
            setup.ipDestino += (UInt32)(buf[i++] << 8);
            setup.ipDestino += (UInt32)(buf[i++]);

            //buf[i++] = (byte)((setup.cfge.dhcp << 2) + setup.cfge.DDNS);
            setup.cfge.DDNS = (byte)(buf[i] & 0x03);
            setup.cfge.dhcp = (byte)((buf[i] & 0x04) >> 2);
            setup.cfge.DDNShabilitado = (byte)((buf[i] & 0x08) >> 3);
            i++;

            setup.toutEthernet1 = buf[i++];
            setup.toutEthernet2 = buf[i++];

            setup.porta1 = (UInt16)(buf[i++] << 8);
            setup.porta1 += (UInt16)buf[i++];
            setup.porta2 = (UInt16)(buf[i++] << 8);
            setup.porta2 += (UInt16)buf[i++];
            setup.porta3 = (UInt16)(buf[i++] << 8);
            setup.porta3 += (UInt16)buf[i++];
            setup.porta4 = (UInt16)(buf[i++] << 8);
            setup.porta4 += (UInt16)buf[i++];

            for (j = 0; j < SIZE_OF_DDNS_USUARIO; j++) setup.DDNSusuario[j] = buf[i++];
            for (j = 0; j < SIZE_OF_DDNS_SENHA; j++) setup.DDNSsenha[j] = buf[i++];
            for (j = 0; j < SIZE_OF_DDNS_DEVICE; j++) setup.DDNSdevice[j] = buf[i++];
            for (j = 0; j < SIZE_OF_USUARIO_LOGIN; j++) setup.usuarioLogin[j] = buf[i++];
            for (j = 0; j < SIZE_OF_SENHA_LOGIN; j++) setup.senhaLogin[j] = buf[i++];
            for (j = 0; j < SIZE_OF_DNS_HOST; j++) setup.dnsHost[j] = buf[i++];
            for (j = 0; j < SIZE_OF_DDNS_HOST; j++) setup.DDNShost[j] = buf[i++];

            setup.toleranciaHorario = buf[i++];

            for (j = 0; j < N_LEITORAS; j++) setup.funcaoLeitora[j] = buf[i++];

            setup.tempoLedBuzzerLeitoras = buf[i++];

            // buf[i++] = (byte)((setup.cfg7.toutMsgDisplay << 4) + (setup.cfg7.avisoLowBat << 3) + setup.cfg7.toutPanico);
            setup.cfg7.toutPanico = (byte)(buf[i] & 0x07);
            setup.cfg7.avisoLowBat = (byte)((buf[i] & 0x08) >> 3);
            setup.cfg7.toutMsgDisplay = (byte)((buf[i] & 0xF0) >> 4);
            i++;

            setup.toutPassback = buf[i++];

            //buf[i++] = (byte)((setup.cfg8.emHorarioVerao << 7) + (setup.cfg8.horarioVeraoHabilitado << 6) + (setup.cfg8.autonomia << 4) + (setup.cfg8.leitoraManchester << 3) + (setup.cfg8.saidaLivre << 2) + setup.cfg8.catraca2010);
            setup.cfg8.catraca2010 = (byte)(buf[i] & 0x03);
            setup.cfg8.saidaLivre = (byte)((buf[i] & 0x04) >> 2);
            setup.cfg8.leitoraManchester = (byte)((buf[i] & 0x08) >> 3);
            setup.cfg8.autonomia = (byte)((buf[i] & 0x30) >> 4);
            setup.cfg8.horarioVeraoHabilitado = (byte)((buf[i] & 0x40) >> 6);
            setup.cfg8.emHorarioVerao = (byte)((buf[i] & 0x80) >> 7);
            i++;

            for (j = 0; j < 4; j++) setup.horarioVerao[j] = buf[i++];

            setup.porta5 = (UInt16)(buf[i++] << 8);
            setup.porta5 += (UInt16)buf[i++];
            setup.porta6 = (UInt16)(buf[i++] << 8);
            setup.porta6 += (UInt16)buf[i++];

            setup.cfg9.catracaAdaptada = (byte)(buf[i] & 0x01);
            setup.cfg9.tratarBiometria_1_1 = (byte)((buf[i] & 0x02) >> 1);
            setup.cfg9.antipassbackDesligado = (byte)((buf[i] & 0x04) >> 2);
            setup.cfg9.umSensorParaDuasLeitoras_1e2 = (byte)((buf[i] & 0x08) >> 3);
            setup.cfg9.considerarWiegand26bits = (byte)((buf[i] & 0x10) >> 4);
            setup.cfg9.alarmeEntradaDigitalAberta = (byte)((buf[i] & 0x20) >> 5);
            setup.cfg9.eventoEntradaDigitalAberta = (byte)((buf[i] & 0x40) >> 6);
            setup.cfg9.umSensorParaDuasLeitoras_3e4 = (byte)((buf[i] & 0x80) >> 7);
            i++;

            setup.porta7 = (UInt16)(buf[i++] << 8);
            setup.porta7 += (UInt16)buf[i++];

            setup.tempoEventoEntradaDigitalAberta = buf[i++];

            setup.cfg10.filtrarEvento0 = (byte)(buf[i] & 0x01);
            setup.cfg10.filtrarEvento1 = (byte)((buf[i] & 0x02) >> 1);
            setup.cfg10.filtrarEvento2 = (byte)((buf[i] & 0x04) >> 2);
            setup.cfg10.filtrarEvento3 = (byte)((buf[i] & 0x08) >> 3);
            setup.cfg10.filtrarEvento4 = (byte)((buf[i] & 0x10) >> 4);
            setup.cfg10.filtrarEvento5 = (byte)((buf[i] & 0x20) >> 5);
            setup.cfg10.filtrarEvento6 = (byte)((buf[i] & 0x40) >> 6);
            setup.cfg10.filtrarEvento7 = (byte)((buf[i] & 0x80) >> 7);
            i++;

            setup.cfg11.filtrarEvento8 = (byte)(buf[i] & 0x01);
            setup.cfg11.filtrarEvento9 = (byte)((buf[i] & 0x02) >> 1);
            setup.cfg11.filtrarEvento10 = (byte)((buf[i] & 0x04) >> 2);
            setup.cfg11.filtrarEvento11 = (byte)((buf[i] & 0x08) >> 3);
            setup.cfg11.filtrarEvento12 = (byte)((buf[i] & 0x10) >> 4);
            setup.cfg11.filtrarEvento13 = (byte)((buf[i] & 0x20) >> 5);
            setup.cfg11.filtrarEvento14 = (byte)((buf[i] & 0x40) >> 6);
            setup.cfg11.filtrarEvento15 = (byte)((buf[i] & 0x80) >> 7);
            i++;

            setup.cfg12.filtrarEvento16 = (byte)(buf[i] & 0x01);
            setup.cfg12.filtrarEvento17 = (byte)((buf[i] & 0x02) >> 1);
            setup.cfg12.filtrarEvento18 = (byte)((buf[i] & 0x04) >> 2);
            setup.cfg12.filtrarEvento19 = (byte)((buf[i] & 0x08) >> 3);
            setup.cfg12.filtrarEvento20 = (byte)((buf[i] & 0x10) >> 4);
            setup.cfg12.filtrarEvento21 = (byte)((buf[i] & 0x20) >> 5);
            setup.cfg12.filtrarEvento22 = (byte)((buf[i] & 0x40) >> 6);
            setup.cfg12.filtrarEvento23 = (byte)((buf[i] & 0x80) >> 7);
            i++;

            setup.toutAlarmeEntradaDigitalAberta = buf[i++];

            setup.cfg13.nivelSegurancaBiometria = (byte)(buf[i] & 0x07);
            setup.cfg13.desvincularVisitante = (byte)((buf[i] & 0x08) >> 3);
            setup.cfg13.enviarEventoIndexado = (byte)((buf[i] & 0x10) >> 4);
            setup.cfg13.validarBiometriaOnline_emRemoto = (byte)((buf[i] & 0x20) >> 5);
            setup.cfg13.controleVagasRota = (byte)((buf[i] & 0x40) >> 6);
            setup.cfg13.bimetria2panico = (byte)((buf[i] & 0x80) >> 7);

            i++;
            setup.toutAcionamentoLedBuzzer = buf[i++];
            setup.portaHttp = (UInt16)(buf[i++] << 8);
            setup.portaHttp += (UInt16)buf[i++];


            setup.cfg14.panicoSaidaDigital = (byte)(buf[i] & 0x03);
            setup.cfg14.cartao2xPanico = (byte)((buf[i] & 0x04) >> 2);
            setup.cfg14.tipoTag_UHF = (byte)((buf[i] & 0x18) >> 3);
            setup.cfg14.varreSimultanea_UHF = (byte)((buf[i] & 0x20) >> 5);
            setup.cfg14.wiegand34_UHF = (byte)((buf[i] & 0x40) >> 6);
            setup.cfg14.frequenciaHopping_UHF = (byte)((buf[i] & 0x80) >> 7);
            i++;

            setup.cfg15.buzzer_L1_UHF_on = (byte)(buf[i] & 0x01);
            setup.cfg15.buzzer_L2_UHF_on = (byte)((buf[i] & 0x02) >> 1);
            setup.cfg15.buzzer_L3_UHF_on = (byte)((buf[i] & 0x04) >> 2);
            setup.cfg15.buzzer_L4_UHF_on = (byte)((buf[i] & 0x08) >> 3);
            setup.cfg15.rele5_eventoClonagem = (byte)((buf[i] & 0x10) >> 4);
            setup.cfg15.rele5_eventoPanico = (byte)((buf[i] & 0x20) >> 5);
            setup.cfg15.rele5_eventoAlarme = (byte)((buf[i] & 0x40) >> 6);
            setup.cfg15.rele5_eventoLigado = (byte)((buf[i] & 0x80) >> 7);
            i++;

            setup.tRele5 = buf[i++];

            setup.cfg16.senha13digitos = (byte)((buf[i]) & 0x01);
            setup.cfg16.tipoCatraca = (byte)((buf[i] >> 1) & 0x0F);
            setup.cfg16.direcaoTurnos = (byte)((buf[i] >> 5) & 0x01);
            setup.cfg16.panicoSenha99 = (byte)((buf[i] >> 6) & 0x01);
            setup.cfg16.desligarBuzzer = (byte)((buf[i] >> 7) & 0x01);
            i++;

            setup.porta8 = (UInt16)(buf[i++] << 8);
            setup.porta8 += (UInt16)buf[i++];

            setup.porta9 = (UInt16)(buf[i++] << 8);
            setup.porta9 += (UInt16)buf[i++];

            setup.porta10 = (UInt16)(buf[i++] << 8);
            setup.porta10 += (UInt16)buf[i++];

            setup.porta11 = (UInt16)(buf[i++] << 8);
            setup.porta11 += (UInt16)buf[i++];

            setup.porta12 = (UInt16)(buf[i++] << 8);
            setup.porta12 += (UInt16)buf[i++];

            setup.porta13 = (UInt16)(buf[i++] << 8);
            setup.porta13 += (UInt16)buf[i++];

            setup.cfg17.baudrateRS485_1 = (byte)(buf[i] & 0x03);
            setup.cfg17.baudrateRS485_2 = (byte)((buf[i] >> 2) & 0x03);
            setup.cfg17.baudrateRS485_3 = (byte)((buf[i] >> 4) & 0x03);
            setup.cfg17.baudrateRS485_4 = (byte)((buf[i] >> 6) & 0x03);
            i++;

            setup.cfg18.retardoDesligarSolenoide = (byte)(buf[i] & 0x03);
            setup.cfg18.desligaRetornoAutoT5S = (byte)((buf[i] >> 2) & 0x01);
            setup.cfg18.facilityWieg66 = (byte)((buf[i] >> 3) & 0x01);
            setup.cfg18.habilitaContagemPassagem = (byte)((buf[i] >> 4) & 0x01);
            setup.cfg18.habilitaAtualizacaoAuto = (byte)((buf[i] >> 5) & 0x01);
            setup.cfg18.habilitaIDMestre = (byte)((buf[i] >> 6) & 0x01);
            setup.cfg18.habilitaAcomodacao = (byte)((buf[i] >> 7) & 0x01);
            i++;

            setup.horarioAtualizaAuto[0] = (byte)(buf[i++]);
            setup.horarioAtualizaAuto[1] = (byte)(buf[i++]);

            setup.cfg19.sensorPassagemCofre = (byte)(buf[i] & 0x01);
            setup.cfg19.sw_watchdog = (byte)((buf[i] >> 1) & 0x01);
            setup.cfg19.verificarFirmwareFTP = (byte)((buf[i] >> 2) & 0x01);
            setup.cfg19.sensorGiroInvertido = (byte)((buf[i] >> 3) & 0x01);
            setup.cfg19.desativaRelePassagem = (byte)((buf[i] >> 4) & 0x01);
            setup.cfg19.antipassbackDesligado_entrada = (byte)((buf[i] >> 5) & 0x01);
            setup.cfg19.antipassbackDesligado_saida = (byte)((buf[i] >> 6) & 0x01);
            setup.cfg19.duplaValidacaoVisitante = (byte)((buf[i] >> 7) & 0x01);
            i++;

            setup.TempoQuedaCartao = (byte)(buf[i++]);

            setup.tRele6 = (byte)(buf[i++]);

            return setup;
        }


        //--------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------
        public byte[] dispositivoParaVetor(byte tipo, UInt64 serial, int contadorHCS, int codHabilitacao, flagsCadastro fCadastro, flagsStatus fStatus, byte nivel, byte creditos, s_validade dataValidade, string userLabel)
        {
            byte[] v = new byte[SIZE_OF_DISPOSITIVO];
            UInt16 i = 0, j;

            v[i] = (byte)(tipo << 4);
            if (tipo == DISP_TX)
            {
                v[i++] |= (byte)((serial >> 24) & 0x0F);
                v[i++] = (byte)(serial >> 16);
                v[i++] = (byte)(serial >> 8);
                v[i++] = (byte)serial;

                v[i++] = (byte)(contadorHCS >> 8);
                v[i++] = (byte)contadorHCS;
            }
            else
            {
                v[i++] |= (byte)((serial >> 40) & 0x0F);
                v[i++] = (byte)(serial >> 32);
                v[i++] = (byte)(serial >> 24);
                v[i++] = (byte)(serial >> 16);
                v[i++] = (byte)(serial >> 8);
                v[i++] = (byte)serial;
            }

            v[i++] = (byte)(codHabilitacao >> 8); // high
            v[i++] = (byte)codHabilitacao; // low

            v[i++] = fCadastro.val;
            v[i++] = fStatus.val;

            v[i++] = nivel;

            v[i++] = creditos;

            v[i++] = dataValidade.diaIni;
            v[i++] = dataValidade.mesIni;
            v[i++] = dataValidade.anoIni;
            v[i++] = dataValidade.diaFim;
            v[i++] = dataValidade.mesFim;
            v[i++] = dataValidade.anoFim;

            if (userLabel == null)
            {
                for (j = 0; j < 14; j++) v[i++] = (byte)' ';
            }
            else
            {
                for (j = 0; j < 14; j++)
                {
                    if (userLabel.Length > j)
                    {
                        v[i++] = (byte)userLabel[j];
                    }
                    else
                    {
                        v[i++] = (byte)' ';
                    }
                }
            }
            return v;
        }



        //--------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------
        public byte[] criaBufferCmd(int tamanho)
        {
            byte[] buf = new byte[tamanho + 8];

            buf[0] = (byte)'S';
            buf[1] = (byte)'T';
            buf[2] = (byte)'X';
            buf[3] = (byte)(tamanho >> 8);
            buf[4] = (byte)tamanho;
            buf[tamanho + 8 - 3] = (byte)'E';
            buf[tamanho + 8 - 2] = (byte)'T';
            buf[tamanho + 8 - 1] = (byte)'X';
            return buf;
        }

        //--------------------------------------------------------------
        //
        //
        //--------------------------------------------------------------
        public byte checksum(byte[] buf, int tamanho)
        {
            int i;
            byte checksum = 0;
            for (i = 0; i < tamanho; i++) checksum += buf[i + 5];
            return checksum;
        }
    }
}
