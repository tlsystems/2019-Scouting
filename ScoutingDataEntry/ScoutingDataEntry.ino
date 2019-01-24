// Visual Micro is in vMicro>General>Tutorial Mode
// 
/*
    Name:       ScoutingDataEntry.ino
    Created:	1/17/2019 1:08:07 PM
    Author:     tls-mobile7\tomh
*/

char VERSION[] = "v0.3";


#define pinBTN1 8
#define pinBTN2 9
#define pinBTN3 7
#define pinBTN4 6
#define pinBTN5 5
#define pinBTN6 4
#define pinBTN7 3
#define pinBTN8 2

#define pinSW0  10
#define pinSW1  11
#define pinSW2  12

#define pinBuzzer 13
#define pinLED 19

enum
{
	eLED_ON,
	eLED_OFF,
	eLED_Toggle
};


uint8_t _switches;
uint8_t _buttons;

void setup()
{
	// buttons
	pinMode(pinBTN1, INPUT_PULLUP);
	pinMode(pinBTN2, INPUT_PULLUP);
	pinMode(pinBTN3, INPUT_PULLUP);
	pinMode(pinBTN4, INPUT_PULLUP);
	pinMode(pinBTN5, INPUT_PULLUP);
	pinMode(pinBTN6, INPUT_PULLUP);
	pinMode(pinBTN7, INPUT_PULLUP);
	pinMode(pinBTN8, INPUT_PULLUP);

	// switches
	pinMode(pinSW0, INPUT_PULLUP);
	pinMode(pinSW1, INPUT_PULLUP);
	pinMode(pinSW2, INPUT_PULLUP);

	pinMode(pinBuzzer, OUTPUT);
	digitalWrite(pinLED, HIGH);
	pinMode(pinLED, OUTPUT);

	Serial.begin(115200);

	_switches = PollSwitches();
}

bool _bFlashLED = false;
bool _bDisplayDebounceCounts = false;
int risingCount[8];
int fallingCount[8];

void loop()
{
	static uint32_t lastMillisFlash = 0;
	static uint32_t lastMillisPrint = 0;
	static char inChar;
	static char responseBuffer[3];

	_buttons  = PollButtons();

	// count button changes to check if they need to be debounced
	if (_bDisplayDebounceCounts &&
		(millis() - lastMillisPrint > 500))
	{
		CheckForBounce(_buttons);
		for(int i = 0; i < 8; i++)
		{
			Serial.print(fallingCount[i]);
			Serial.print(' ');
			Serial.print(risingCount[i]);
			Serial.print(' ');
		}
		Serial.println();
		lastMillisPrint = millis();
	}
    
	// flash LED if enabled
	if (_bFlashLED && (millis() - lastMillisFlash > 500))
	{
		lastMillisFlash = millis();
		SetLED(eLED_Toggle);
	}
	
		
	// check for command on serial port
	if (Serial.available() > 0)
	{
		switch(Serial.read())
		{
		case 'd':	// control debounce counters display
			_bDisplayDebounceCounts = (inChar == '1');
			Serial.println(_bDisplayDebounceCounts);
			break;

		case 'l':	// set LED state
			Serial.readBytes(&inChar, 1);
			_bFlashLED = false;
			switch (inChar)
			{
			case '0':
				SetLED(eLED_OFF);
				_bFlashLED = false;
				break;
			case '1':
				SetLED(eLED_ON);
				_bFlashLED = false;
				break;
			case 'f':
				_bFlashLED = true;
				break;
			default:
				break;
			}
			break;

		case 'p':	// poll switches/buttons
			Serial.readBytes(&inChar, 1);
			if (inChar == 's')
				FormatResponse(_switches, responseBuffer);
			else if (inChar == 'b')
				FormatResponse(_buttons, responseBuffer);
			Serial.println(responseBuffer);
			break;

		case 'v':
			Serial.println(VERSION);
			break;

		default:
			break;
		}
	}
}

void SetLED(int action)
{
	switch (action)
	{
		case eLED_ON:
			digitalWrite(19, LOW);
			break;
		case eLED_OFF:
			digitalWrite(19, HIGH);
			break;
		case eLED_Toggle:
			digitalWrite(19, !digitalRead(19));
			break;
		default:
			break;
	}
}

uint16_t PollButtons()
{
	uint16_t data = 0;
	if (!digitalRead( 8)) data |= 0x01;
	if (!digitalRead( 9)) data |= 0x02;
	if (!digitalRead( 7)) data |= 0x04;
	if (!digitalRead( 6)) data |= 0x08;
	if (!digitalRead( 5)) data |= 0x10;
	if (!digitalRead( 4)) data |= 0x20;
	if (!digitalRead( 3)) data |= 0x40;
	if (!digitalRead( 2)) data |= 0x80;
	return data;
}

uint16_t PollSwitches()
{
	uint8_t data = 0;
	if (!digitalRead(10)) data |= 0x01;
	if (!digitalRead(11)) data |= 0x02;
	if (!digitalRead(12)) data |= 0x04;
	return data;
}

void FormatResponse(uint16_t inputData, char* pBuffer)
{
	*pBuffer++ = Bin2Hex((inputData >> 4) & 0x0f);
	*pBuffer++ = Bin2Hex((inputData >> 0) & 0x0f);
	*pBuffer = '\0';
}

char Bin2Hex(byte inData)
{	
	inData &= 0x0f;
	return (inData <= 9) ? inData + '0' : inData - 10 + 'A';
}
void CheckForBounce(byte currentState) 
{
  static byte lastState = 0;
  byte changed = currentState ^ lastState;
  lastState = currentState;
  
  for(int i = 0; i < 8; i++)
  {
    if(changed & 0x01)
    {
      if(currentState & 0x01) 
        risingCount[i]++;
      else                    
        fallingCount[i]++;
    }
    changed      >>= 1;      
    currentState >>= 1;
  }
}
