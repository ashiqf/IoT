#define BaudRate 9600

char inputValue;
int led1 = 13;

void setup() {
  // Initialize serial communication at 9600 bits per second
  Serial.begin(BaudRate);
  // Prepare the digital output pins
  pinMode(led1, OUTPUT);
  // Initially all are off
  digitalWrite(led1, LOW);
}

// the loop function runs over and over again forever
void loop() 
{
  // Reads the input
  inputValue = Serial.read();
  if(inputValue == 'A')
  {
    // Turn on the LED  
    digitalWrite(led1, HIGH);   
  }
  else if (inputValue == 'B') 
  {
    // Turn off the LED
    digitalWrite(led1, LOW);     
  }
}