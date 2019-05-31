/***
   Craos PWM
*/
#include <SPI.h>
#include <EtherCard.h>
#include <Wiegand.h>


static byte _macAddress[] = { 0x00, 0xAD, 0xBE, 0xEF, 0xFE, 0x17 }; // Endereço de rede
static byte _currentIP[] = { 192, 168, 11, 65 };                    // Endereço IP
static byte _gateWay[] = { 192, 168, 11, 180 };                     // Endereço do Gateway
static byte hisip[] = { 192, 168, 11, 180 };                        // Endereço do servidor para requisições HTTP
const char website[] PROGMEM = "192.168.11.180";                    // Endereço do servidor para requisições HTTP (String de requisição)
String _equipamento = "&eq=2508";                                   // Número do equipamento na rede
static BufferFiller bfill;                                          // Ponteiro dos bytes de entrada (Entrada nas requisições remoto -> host)
byte Ethernet::buffer[500];                                         // Ponteiro dos bytes de entrada (Retorno de requisições host -> remoto)
WIEGAND leitorWiegand;                                              // Objeto de leitura Wiegand
const int period = 3000;                                            // Intervalo de acionamento dos reles
unsigned long time_now = 0;                                         // Obtem o tempo corrente desde o inicio da operacao

int sensor_porta = 9;                                               // Numero da porta do sensor
int sensor_contador = 0;                                            // Faz a contagem de tempo do sensor de porta aberta
int sensor_aviso = 20000;                                           // Se a variável sensor_contador atingir este limite, emite o sinal sonoro
int sensor_contadorfechada = 0;                                     // Faz a contagem de tempo para encerrar o buzzer
int sensor_portafechada = 2000;                                     // Tempo final para encerrar o buzzer



void setup() {

  Serial.begin(9600);

  ether.begin(sizeof Ethernet::buffer, _macAddress);
  ether.staticSetup(_currentIP, _gateWay);
  ether.copyIp(ether.hisip, hisip);
  //ether.printIp("Servidor: ", ether.hisip);

  leitorWiegand.begin();
  
  pinMode(5, OUTPUT);
  pinMode(6, OUTPUT);
  pinMode(8, OUTPUT);
  pinMode(sensor_porta, INPUT_PULLUP);
  
}

void loop() {

  time_now = millis();

  // Faz a leitura dos pacotes HTTP
  word len = ether.packetReceive();
  word pos = ether.packetLoop(len);

  if (pos) {
    char* data = (char *) Ethernet::buffer + pos;
    if (strncmp("GET /?r=0", data, 9) == 0) {
      RespondeHttpGet();
      AcionaRele(5);
      AcionaRele(6);

    } else if (strncmp("GET /?r=5", data, 9) == 0) {
      RespondeHttpGet();
      AcionaRele(5);

    } else if (strncmp("GET /?r=6", data, 9) == 0) {
      RespondeHttpGet();
      AcionaRele(6);

    } else {
       RespondeHttpGet();
      AcionaRele(8);
    }
  }

  // Faz a leitura do codigo RFID
  if (leitorWiegand.available()) {

    String codigo = String(leitorWiegand.getCode(), HEX);
    Serial.println(codigo);

    if (Serial.available() == 0) {
      if (codigo.length() >= 5) {
        EnviaSolicitacaoPost(codigo + _equipamento);
      }
    }
  }

  // Escuta a comunicação serial para acionar rele (Verifique a tabela Ascii)
  if (Serial.available() > 0) {
    int comando = Serial.read();
    if (comando == 53) {
      AcionaRele(5);
    } else if (comando == 54) {
      AcionaRele(6);
    } else if (comando == 56) {
      AcionaRele(8);
    }
    comando = 0;
  }

  // Monitora o sensor de porta
  //MonitoraSensor();
  
}

/***
   Envia uma página para finalizar a requisição remota.
*/
static void RespondeHttpGet() {
  const char pageA[] PROGMEM =
    "HTTP/1.0 200 OK\r\n"
    "Content-Type: text/html\r\n"
    "Retry-After: 600\r\n";

  ether.httpServerReplyAck();
  ether.httpServerReply_with_flags(sizeof pageA - 1, TCP_FLAGS_ACK_V);

}

/***
   Envia uma solicitação para o Webservice
*/
static void EnviaSolicitacaoPost (String cartao) {
  char cd[23];
  cartao.toCharArray(cd, 23);
  const char *codigo = cd;

  // c=312 -> comando do webservice
  // cn=as -> banco de dados
  // li=1  -> Leitor RFID
  ether.browseUrl(PSTR("/ws/?c=312&cn=as&li=1&rfid="), codigo, website, callback);
}

/***
   Recebe as informacoes do Webservice
*/
static void callback (byte status, word off, word len) {

  Ethernet::buffer[off + 300] = 0;
  const char *buf = (const char*) Ethernet::buffer + off;
  String resultado = buf;
  if (resultado.indexOf("|") > -1) {
    AcionaRele(6);
  }
}

static void AcionaRele(int Rele) {

  while (millis() < time_now + period) {
    digitalWrite(Rele, HIGH);
  }
  digitalWrite(Rele, LOW);

}

static void MonitoraSensor() {

  if (digitalRead(sensor_porta) == 1) {
      sensor_contador++;
  } else {
    sensor_contadorfechada++;
  }

  if (sensor_contador == sensor_aviso) {
    AcionaRele(8);
  }

  if (sensor_contadorfechada == sensor_portafechada) {
    sensor_contador = 0;
  }

  
}
