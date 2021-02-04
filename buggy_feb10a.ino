//config
int BaudR = 9600;
int sonicTests = 3;
int sonicThreshold = 10;
int timeOut = 100;
int buggy_ID = 1;
int sup_ID = 2;
String name = "Buggy";

//pins
const int irLED    = 2;//ir inturupt = 0;
const int sonicGND = 16;
const int sonicVCC = 17;
const int sonicSIG = 18;
const int _pin = sonicSIG;
const int overridePin = 6;


//functions
void DistanceMeasure(void);
long microsecondsToCentimeters(void);
void gantryDectected();
void sonicSense();
void sonicDetected();
void readIn();
void sendOut();
void executeCMD(int CMD);

//vars
long duration;
long distance;
int messOut;
int messIn;


//Pulses
int Stop =       2;
int Normal =     4;
int LeftO =      6;
int RightO =     8;
int RotateLeft = 10;
int ReduceP =    12;
int IncreaceP =  14;
int HalfP =      16;
int FullP =      18;

//Comands Out
int gantry1_CMD =   1;         //gantry 1-4
int gantry2_CMD =   2;
int gantry3_CMD =   3;
int sonic_CMD =     5;      // sonic = 5

//In Variables
char forBuggy;
char buggy;
char command;
char cNumber;
String message;
String inputString;
bool stringComplete;

void setup() {
 //Start Serial
 Serial.begin(BaudR);
 Serial.print("+++");//xbee config
 delay(1500);//gaurd time
 Serial.println("ATID 2552, CH C, CN");
 //AT commands used are: ID =PAN ID, CH = Channel and CN = escape AT command mode

 inputString.reserve(200);

 //pinModes
 pinMode(sonicGND, OUTPUT);
 pinMode(sonicVCC, OUTPUT);
 pinMode(irLED, INPUT);
 attachInterrupt(0, gantryDectected, RISING);

 //initial values
 messOut = 0;
 messIn = 0;
 duration = 0;
 distance = 0;
}

//main +++++++++++++++++++++++++++++++++
void loop() 
{

  int _continue = 1;

  //arduino sending
  while(_continue == 0)
  {
    for(int i = 1; i < 5; i++)
    {
      for(int j = 1; j<5; j++)
      {
        Serial.print(name);
        Serial.print(" b");
        Serial.print(i);
        Serial.print("g");
        Serial.print(j);
        Serial.print("\n");

        delay(1500);
      }
    }
  }

  //arduino read/write
  while(_continue == 1)
  {
    while(Serial.available() == 0);
    
    message = Serial.readStringUntil('\n');
    
    Serial.print(name);
    Serial.print(" heard: ");
    Serial.println(message);
  }

  //arduino send distance
  while(_continue == 2)
  {
    digitalWrite(sonicGND, LOW);
    digitalWrite(sonicVCC, HIGH);

     DistanceMeasure();
     distance = microsecondsToCentimeters();
     
    Serial.print(name);
    //Serial.print(" distance: ");
    Serial.println(distance);
    delay(1500);
  }

  //main
  while(_continue == 3)
  {
    if (stringComplete) {
    Serial.println(inputString);
    // clear the string:
    inputString = "";
    stringComplete = false;
  }
  }

  //arduino parse
  while(_continue ==4)
  {
    readIn();
    delay(1000);
  }
}

//++++++++++++++++++++++++++++++++++++++

//functions ----------------
void executeCMD(int CMD)
{
  digitalWrite(overridePin, HIGH);
  delay(CMD);
  digitalWrite(overridePin, LOW);
}

void sendOut(int sender, int receipient, int CMD)
{
  bool cont = true;
  messOut++;
 
  while(cont == true)
  {
    cont = false;

    Serial.print("r");
    Serial.print(receipient);
    Serial.print("s");
    Serial.print(sender);
    Serial.print("c");
    Serial.print(CMD);
    Serial.print("m");
    Serial.println(messOut);
        
    delay(timeOut);
    
    if(Serial.available() == 0)
    {
      cont = true;
    }
    else
    {
      String received = Serial.readStringUntil('\n');
      if(received[1] == messOut, received[0] == buggy_ID)
      {
        cont = false;
      }
      else
      {
        cont = true;
      }
    }
  }
}

void serialEvent() {
    inputString = Serial.readStringUntil('\n');
    stringComplete = true;
}

//void readIn(){
//  if(Serial.available() != 0)
//  {
//    message = Serial.readStringUntil('\n');
//      
//    Serial.print("Buggy received: ");
//    Serial.println(message);
//    forBuggy = (message[0]);
//    buggy = (message[1]);
//    command = (message[2]);
//    cNumber= (message[3]);
//
//    Serial.print("That means ");
//    if(forBuggy =='B')
//    {
//      Serial.print("buggy ");
//    }
//    else if(forBuggy == 'S')
//    {
//      Serial.print("supervisor ");
//    }
//    
//    Serial.print(buggy);
//    Serial.print(" needs to execute command ");
//    Serial.print(command);
//    Serial.println(cNumber);
//    }
//}
//--------------------------------
//sonic sensor *********************
void DistanceMeasure(void)
{
  pinMode(_pin, OUTPUT);
  digitalWrite(_pin, LOW);
  delayMicroseconds(2);
  digitalWrite(_pin, HIGH);
  delayMicroseconds(5);
  digitalWrite(_pin,LOW);
  pinMode(_pin,INPUT);
  duration = pulseIn(_pin,HIGH);
}

long microsecondsToCentimeters(void)
{
  return duration/29/2; 
}
//********************************

//gantry $$$$$$$$$$$$$$$$$$$$$$$$$
void gantryDectected()
{
  executeCMD(Stop);// stops the buggy
  while(irLED == HIGH);//waits for the ir to go low
  int duration = pulseIn(irLED, HIGH);//measures the length of the pulse of the IR
  
  //checks which gantry we're under
  int gantryNum = gantryNumber(duration);
  int CMD = gantryNum;
  
  sendOut(buggy_ID, sup_ID, CMD); 
}

int gantryNumber(int duration)
{
  if(duration > 500 && duration < 1500)
  {
    return 1;
  }
  else if(duration > 1500 && duration < 2500)
  {
    return 2;
  }
  else if(duration > 2500 && duration < 3500)
  {
    return 3;
  }
  else
  {
    return 0;
  }
}
//$$$$$$$$$$$$$$$$$$$$$$$$$

//sonic ^^^^^^^^^^^^^^^^^^^^^^^^^^^
void sonicSense()
  {
    digitalWrite(sonicGND, LOW);
    digitalWrite(sonicVCC, HIGH);
    distance = 0;

    for(int i = 0; i < sonicTests; i++)
    {
      DistanceMeasure();
      distance += microsecondsToCentimeters();
      DistanceMeasure();
      distance += microsecondsToCentimeters();
      DistanceMeasure();     
      distance += microsecondsToCentimeters();
    }
    
    distance = distance/sonicTests;

    if(distance < sonicThreshold)
    {
      sonicDetected();
    }
  }

void sonicDetected()
{
  executeCMD(Stop);// stops the buggy
  int CMD = sonic_CMD;
  
  sendOut(buggy_ID, sup_ID, CMD); 
}
//^^^^^^^^^^^^^^^^^^^^^^^^^^^
