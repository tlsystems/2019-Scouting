// Visual Micro is in vMicro>General>Tutorial Mode
// 
/*
    Name:       ScoutingDataEntry.ino
    Created:	1/17/2019 1:08:07 PM
    Author:     tls-mobile7\tomh
*/

enum
{
	eLED_ON,
	eLED_OFF,
	eLED_Toggle
};

bool _bFlashLED = false;

void setup()
{
	// buttons
	pinMode(2, INPUT_PULLUP);
	pinMode(3, INPUT_PULLUP);
	pinMode(4, INPUT_PULLUP);
	pinMode(5, INPUT_PULLUP);
	pinMode(6, INPUT_PULLUP);
	pinMode(7, INPUT_PULLUP);
	pinMode(8, INPUT_PULLUP);
	pinMode(9, INPUT_PULLUP);

	// switches
	pinMode(10, INPUT_PULLUP);
	pinMode(11, INPUT_PULLUP);
	pinMode(12, INPUT_PULLUP);
	pinMode(13, OUTPUT);
	pinMode(19, OUTPUT);
	digitalWrite(19,HIGH);

	Serial.begin(115200);
}


void loop()
{
	static uint32_t lastMillis = 0;
	static uint8_t buttons;
	static uint8_t switches;
	static char responseBuffer[3];

	buttons  = PollButtons();
	switches = PollSwitches();

	if (_bFlashLED && (millis() - lastMillis > 500))
	{
		lastMillis = millis();
		SetLED(eLED_Toggle);
	}
	
	if (Serial.available() > 0)
	{
		switch(Serial.read())
		{
		case 's':
			FormatResponse(switches, responseBuffer);
			Serial.println(responseBuffer);
			SetLED(eLED_Toggle);
			break;

		case 'b':
			FormatResponse(buttons, responseBuffer);
			Serial.println(responseBuffer);
			break;

		case '0':
			_bFlashLED = false;
			SetLED(eLED_OFF);
			break;

		case '1':
			_bFlashLED = false;
			SetLED(eLED_ON);
			break;

		case 'f':
			_bFlashLED = true;
			break;

		case 'v':
			Serial.println("v0.02");
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
