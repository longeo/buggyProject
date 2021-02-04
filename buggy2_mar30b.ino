//config
const int BaudR = 9600;
const long thresholdDistance = 10;
const long P1thresholdDistance = 60;
const long P2thresholdDistance = 20;
const long FThresholdDistance = 15;
const int timeOut = 100;
const char buggy_ID = '2';
const char sup_ID = '1';
const long startParkingTime = 6000;
const long parkingTime = 3500;
const long sonic_interval = 200;           // interval at which to pole sonic
const long sonic_threshold = 1000;
const long gantry_interval = 50;
const long turnTime = 800;
const unsigned long lastGantryInterval = 3000;

String name = "Buggy";

//pins
const int ledPin =  13;
const int irLED    = 2;//ir inturupt = 0;
const int sonicGND = 16;
const int sonicVCC = 17;
const int sonicSIG = 18;
const int _pin = sonicSIG;
const int overridePin = 3;

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

//functions
long DistanceMeasure(void);
long microsecondsToCentimeters(long duration);
int sonicSense();
void gantryDectected();
void sonicDetected();
void readIn();
void sendOut();
void executeCMD(int CMD);
void changeMode(int Mode);

//vars
//long duration;
//long distance;
int messOut;
int messIn;
unsigned long lastGantry = 0;
unsigned long previousMillis = 0;        // will store last time LED was updated
unsigned long previousSonic = 0;
unsigned long startedParking = 0;
int currentThreshold = thresholdDistance; 
int ledState = LOW;
volatile bool underGantry = false;
volatile bool checked = false;
bool stoptedObj = false;
bool parking = false;
bool overrideOn = false;
bool rotating = false;
int over = RightO;
int mode = 1; // 1 =normal, 2 = parking1, 3 = parking2, 4 = follow

//Comands Out
int gantry1_CMD =   1;         //gantry 1-4
int gantry2_CMD =   2;
int gantry3_CMD =   3;
int no_sonic_CMD =  4;
int sonic_CMD =     5;      // sonic = 5

//In Variables
char forBuggy;
char buggy;
char command;
char cmd1;
char cmd2;
char fromSup;
char sup;
//String message;
String inputString = "";
bool stringComplete = false;

void setup() {
 //Start Serial
 Serial.begin(BaudR);
 Serial.print("+++");//xbee config
 delay(1500);//gaurd time
 Serial.println("ATID 3302, CH C, CN");
 //AT commands used are: ID =PAN ID, CH = Channel and CN = escape AT command mode
 previousMillis = millis();
 unsigned long currentMillis = millis();
 
 inputString.reserve(200);

 //pinModes
 pinMode(sonicGND, OUTPUT);
 pinMode(sonicVCC, OUTPUT);
 pinMode(overridePin, OUTPUT);
 pinMode(irLED, INPUT);
 attachInterrupt(0, gantryDectected, RISING);

 //initial values
 messOut = 0;
 messIn = 0;
 previousSonic = millis();

 while(Serial.available()> 0){
  Serial.read();
 }
 
}

//main +++++++++++++++++++++++++++++++++
void loop() 
{
    if (stringComplete) {
    readIn(inputString);
    inputString = "";
    stringComplete = false;
    }

    unsigned long currentMillis = millis();

    if (currentMillis - previousMillis >= gantry_interval)
    {
      if (underGantry && !checked && (currentMillis - lastGantry > lastGantryInterval)) {
        checked = true;
        executeCMD(Stop);// stops the buggy
        while(irLED == HIGH);//waits for the ir to go low
        int duration = pulseIn(irLED, HIGH);//measures the length of the pulse of the IR
        //checks which gantry we're under
        int gantryNum = gantryNumber(duration);
        int CMD = gantryNum;
        if(mode != 4)
        {
          delay(1000);
        }
        sendOut(buggy_ID, sup_ID, CMD); 
      }
      else
      {
        underGantry = false;
        checked = true;
      }
    }
    
    currentMillis = millis();

  if (currentMillis - previousMillis >= sonic_interval) {
    // save the last time you poled sonic
    previousMillis = currentMillis;
    //sonic sense
    sonicSense();
  }

 
    if(overrideOn == true && (currentMillis - startedParking >= parkingTime))
    {
      executeCMD(Normal);
      overrideOn = false;
    }

    if(parking == true && (currentMillis - startedParking >= startParkingTime))
    {
      executeCMD(FullP);
      executeCMD(FullP);
      parking = false;
      if(mode == 2 || mode == 8)
    {
        currentThreshold = P1thresholdDistance;

    }else if(mode == 3 || mode == 9)
    {
        currentThreshold = P2thresholdDistance;
    }
    }
}

//++++++++++++++++++++++++++++++++++++++

//functions ----------------
void executeCMD(int CMD)
{
  if(CMD != Stop  && (underGantry == true))
  {
    underGantry = false;
  }
  digitalWrite(overridePin, LOW);
  delay(2);
  digitalWrite(overridePin, HIGH);
  delay(CMD);
  digitalWrite(overridePin, LOW);
  delay(2);
}

void changeMode(int Mode)
{
  mode = Mode;
  switch(mode){
    case 1:
    currentThreshold = thresholdDistance;
    break;
    case 2:
    startedParking = millis();
    //currentThreshold = P1thresholdDistance;
    executeCMD(over);
    overrideOn = true;
    parking = true;
    break;
    case 3:
    //currentThreshold = P2thresholdDistance;
    startedParking = millis();
    executeCMD(over);
    overrideOn = true;
    parking = true;
    break;
    case 4:
    currentThreshold = 0;//following
    executeCMD(Normal);
    break;
    case 5:
    executeCMD(RotateLeft);
    delay(turnTime);
    executeCMD(Stop);
    mode = 1;
    break;
    case 6:
    executeCMD(Normal);
    delay(2000);
    executeCMD(RightO);
    delay(1000);
    executeCMD(Normal);
    break;
    case 7:
    executeCMD(Normal);
    break;
    case 8:
    startedParking = millis();
    //currentThreshold = P1thresholdDistance;
    executeCMD(LeftO);
    overrideOn = true;
    parking = true;
    break;
    case 9:
    startedParking = millis();
    //currentThreshold = P1thresholdDistance;
    executeCMD(LeftO);
    overrideOn = true;
    parking = true;
    break;
    
  }
}

void sendOut(char sender, char receipient, int CMD)
{
    Serial.print("S");
    Serial.print(receipient);
    Serial.print("B");
    Serial.print(sender);
    Serial.print("C");
    Serial.println(CMD);
    if(CMD == 1 || CMD == 2 || CMD == 3)
    {
      lastGantry = millis();
    }
}

void serialEvent() 
{
    inputString = Serial.readStringUntil('\n');
    stringComplete = true;
}

void readIn(String message){
if(message.length() == 7)
  {
    //echo
    String buggyM = "Buggy 2 Received: ";
    String output = buggyM + message;
    Serial.println(output);
    //eo echo
    
    forBuggy = (message[0]);
    buggy = (message[1]);
    fromSup = (message[2]);
    sup = (message[3]);
    command = (message[4]);
    cmd1= (message[5]);
    cmd2= (message[6]);

    if(forBuggy == 'B' && buggy == buggy_ID)
    {
      int commandInt1 = cmd1 -'0';
      int commandInt2 = cmd2 -'0';
      int commandInt = (commandInt1*10) + commandInt2;
      if(commandInt > 0 && commandInt < 20)
      {
        if(command == 'C')
        {
          executeCMD(commandInt);
          executeCMD(commandInt);
        }
        else if(command == 'c')
        {
          changeMode(commandInt);
          changeMode(commandInt);
        }
      }
    }

  }
}
//--------------------------------
//sonic sensor *********************
int sonicSense()
{
    digitalWrite(sonicGND, LOW);
    digitalWrite(sonicVCC, HIGH);
    long duration = DistanceMeasure();
    long distance = microsecondsToCentimeters(duration);
    unsigned long curr = millis();

    
    
    if(stoptedObj)
    {
      if(distance < currentThreshold)
      {
        previousSonic = millis();
      }
      else if(curr - previousSonic  > sonic_threshold)
      {
        stoptedObj = false;
        int CMD = no_sonic_CMD;
        sendOut(buggy_ID, sup_ID, CMD);
      }
    }
    else
    {
      int sonicS = distance;
      if(sonicS < thresholdDistance) // must stop
      {
        sonicDetected();
      }
      else if(sonicS < currentThreshold) //parking
      {
        sonicDetected();
        if(mode == 2 || mode == 3)
        {
          changeMode(1);
          parking = false;
        }
      }
      else if(sonicS < FThresholdDistance && mode == 4)
      {
        executeCMD(ReduceP);
      }
      else if(sonicS > FThresholdDistance && mode == 4)
      {
        executeCMD(Normal);
        executeCMD(IncreaceP);
      }
      
      if(distance < currentThreshold)
      {
        stoptedObj = true;
        //trouble shooting for distance
//        Serial.println("Object detected!!");
//        Serial.print("Distance: ");
//        Serial.println(distance);
      }
    }
    return distance;
}

void sonicDetected()
{
    executeCMD(Stop);// stops the buggy
    int CMD = sonic_CMD;
    sendOut(buggy_ID, sup_ID, CMD);
}

long DistanceMeasure(void)
{
  pinMode(_pin, OUTPUT);
  
  digitalWrite(_pin, LOW);
  delayMicroseconds(2);
  digitalWrite(_pin, HIGH);
  delayMicroseconds(5);
  digitalWrite(_pin,LOW);
  
  pinMode(_pin,INPUT);
  
  return pulseIn(_pin,HIGH);
}

long microsecondsToCentimeters(long duration)
{
  return duration/29/2; 
}
//********************************

//gantry $$$$$$$$$$$$$$$$$$$$$$$$$
void gantryDectected()
{
  //Serial.println("under g");
  if(!underGantry)
  {
    underGantry = true;
    checked = false;
  }
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
