using System;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Program
{
	class MainClass
	{
		//fixed
		static char sup_ID = '1';
		static int buggy1_ID = 1;
		static int buggy2_ID = 2;
		static SerialPort _serialPort;

		//flags
		static bool _continue;
		static bool _messageReceived;

		//strings
		static string Message;//message we type
		static string name;//note used
		static string message;//message the buggy sends us

		//vars
		public static int messOut = 0;//not used
		public static int LocB1 = 4;//location of buggy 1
		static int LocB2 = 5;//location of buggy 2
		static int Mode = 0;
		static int Lap1 = 1;
		static int Lap2 = 1;
		static bool b1under = false;
		static bool b2under = false;
		static bool b1Obj = false;
		static bool b2Obj = false;

		//execute command values (Big C)
		static int go1 = 0;
		static int go2 = 4;
		static int stop1 = 0;
		static int stop2 = 2;
		static int RightO1 = 0;
		static int RightO2 = 6;
		static int Half01 = 1;
		static int Half02 = 6;
		static int Reduce01 = 1;
		static int Reduce02 = 2;

		//mode values (little c)
		static int normal1 = 0;
		static int normal2 = 1;
		static int park11 = 0;
		static int park12 = 3;
		static int park21 = 0;
		static int park22 = 2;
		static int park212 = 0;
		static int park222 = 8;
		static int park112 = 0;
		static int park122 = 9;
		static int following1 = 0;
		static int following2 = 4;
		static int turn1 = 0;
		static int turn2 = 5;

		static StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;

		static void Main(string[] args)
		{
			int buggy;//not used
			int command;//not used

			//string comparer for "quit"
			//StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
			//thread to read in, will probably use event handler instead
			Thread readThread = new Thread(Read);//create read thread
			Thread writeThread = new Thread(Write);//create write thread

			// Create a new SerialPort object with default settings.
			_serialPort = new SerialPort();//create serial port

			// Allow the user to set serial properties
			_serialPort.PortName = SetPortName(_serialPort.PortName);//sets port name
//			_serialPort.PortName = "/dev/tty.usbserial-A702LD9G";
//			_serialPort.BaudRate = SetPortBaudRate(_serialPort.BaudRate);
//			_serialPort.Parity = SetPortParity(_serialPort.Parity);
//			_serialPort.DataBits = SetPortDataBits(_serialPort.DataBits);
//			_serialPort.StopBits = SetPortStopBits(_serialPort.StopBits);
//			_serialPort.Handshake = SetPortHandshake(_serialPort.Handshake);

			// Set the read/write timeouts
			_serialPort.ReadTimeout = 500;
			_serialPort.WriteTimeout = 500;

			//open the serial port that was just set up
			_serialPort.Open();

			//xbee
			_serialPort.Write("+++");//xbee config
			Thread.Sleep(2000);
			//delay(1500);//gaurd time
			_serialPort.WriteLine("ATID 3302, CH C, CN");
			//AT commands used are: ID =PAN ID, CH = Channel and CN = escape AT command mode

			//handler
			//_serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

			_continue = true;
			readThread.Start();
			writeThread.Start();

			//asks user for name, not used here as the sup id is predefined
			//Console.Write("Name: ");
			//name = Console.ReadLine();
			Console.WriteLine("Type QUIT to exit");

			//The stuff
			while (_continue == true) {
				if (Mode == 1) 
				{
					while (Mode == 1) {
						if (_messageReceived) {
							readIn (Message, Mode);
						}
					}
				} 
				else if (Mode == 2) 
				{
					while (Mode == 2) {
						if (_messageReceived) {
							readIn (Message, Mode);
						}
					}
				} 
				else if (Mode == 3) 
				{
					while (Mode == 3) {
						if (_messageReceived) {
							readIn (Message, Mode);
						}
					}
				} 
				else if (Mode == 4) 
				{
					_continue = false;
				}
			}
			//end of the stuff


			readThread.Join();
			writeThread.Join();
			_serialPort.Close();
		}

		//handler
		private static void DataReceivedHandler(
			object sender,
			SerialDataReceivedEventArgs e)
		{
			SerialPort sp = (SerialPort)sender;
			string indata = sp.ReadLine();
			Console.WriteLine("Data Received:");
			Console.Write(indata);
		}

		public static void readIn(String message, int mode){
			if(message.Length == 7)
			{
				//echo
				String buggyM = "Sup Received: ";
				String output = buggyM + message;
//				_serialPort.WriteLine(output);
				Console.WriteLine (output);
				//eo echo

				char forSup = (message[0]);
				char sup = (message[1]);
				char fromBuggy = (message[2]);
				char buggy_ID = (message[3]);
				char command = (message[4]);
				char cmd1= (message[5]);
				//cmd2= (message[6]);

				if(forSup == 'S' && sup == sup_ID)
				{
					int commandInt = cmd1 -'0';
					int buggy_ID1 = buggy_ID - '0';
					if(commandInt > 0 && commandInt < 20)
					{
						if(command == 'C' && fromBuggy == 'B')
						{
							executeCMD(commandInt, buggy_ID1, mode);
							//executeCMD(commandInt);
						}
//						else if(command == 'c')
//						{
//							changeMode(commandInt);
//							changeMode(commandInt);
//						}
					}
				}

			}
			Message = "";
			_messageReceived = false;
		}

		public static void Read()
		{
			while (_continue)
			{
				try
				{
					Message = _serialPort.ReadLine();
					Console.WriteLine(Message);
					_messageReceived = true;
				}
				catch (TimeoutException) { }
			}
		}

		public static void Write()
		{
			while (_continue)
			{
				try
				{
					String message = Console.ReadLine();

					if (stringComparer.Equals("quit", message))
					{
						_continue = false;
					}
					else if (stringComparer.Equals("bronze", message))
					{
						Mode = 1;
						Lap1 = 1;
						Lap2 = 1;
						LocB1 = 3;
						LocB2 = 0;
						Message = "";
						_messageReceived = false;
						Thread.Sleep(1000);
						_serialPort.WriteLine(string.Format ("B{0}S{1}C{2}{3}", '1', sup_ID, go1, go2));
						//_serialPort.WriteLine(string.Format ("B{0}S{1}C{2}{3}", '1', sup_ID, RightO1, RightO2));

						//_serialPort.WriteLine(string.Format ("B{0}S{1}C{2}{3}", '1', sup_ID, go1, go2));


					}
					else if (stringComparer.Equals("silver", message))
					{
						Mode = 2;
						Lap1 = 1;
						Lap2 = 1;
						LocB1 = 3;
						LocB2 = 0;
						_serialPort.WriteLine(string.Format ("B{0}S{1}C{2}{3}", '1', sup_ID, go1, go2));
					}
					else if (stringComparer.Equals("gold", message))
					{
						Mode = 3;
						Lap1 = 1;
						Lap2 = 1;
						LocB1 = 3;
						LocB2 = 0;
						b2under = true;
						_serialPort.WriteLine(string.Format ("B{0}S{1}C{2}{3}", '1', sup_ID, go1, go2));

//						_serialPort.WriteLine(string.Format ("B{0}S{1}C{2}{3}", '2', sup_ID, Reduce01, Reduce02));
//						_serialPort.WriteLine(string.Format ("B{0}S{1}C{2}{3}", '2', sup_ID, Reduce01, Reduce02));

					}
					else
					{
						_serialPort.WriteLine(message);
					}
				}
				catch (TimeoutException) { }
			}
		}

		public static void executeCMD(int _cmd, int _buggy, int _mode)
		{
			if (_mode == 1) {
				switch (_cmd) {
				case 5:
					Console.WriteLine ("sup: \"buggy stopped at obj \" ");
					break;
				case 4:
					Console.WriteLine ("sup: \"object has gone \" ");
					_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
					_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
					break;
				case 3:
					Console.WriteLine ("sup: \"buggy at gantry 3 \" ");
					_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
					_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
					break;
				case 2:
					Console.WriteLine ("sup: \"buggy at gantry 2 \" ");
					_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
					_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
					break;
				case 1:
					Console.WriteLine ("sup: \"buggy at gantry 1 \" ");
					Lap1 = Lap1 + 1;
					if (Lap1 < 4) {
						Console.WriteLine ("sup: \"lap < 4 \" ");
						_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
						_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
					} else if (Lap1 == 4) {
						Console.WriteLine ("sup: \"lap = 4 \" ");
						_serialPort.WriteLine (string.Format ("B{0}S{1}c{2}{3}", _buggy, sup_ID, park11, park12));
						_serialPort.WriteLine (string.Format ("B{0}S{1}c{2}{3}", _buggy, sup_ID, park11, park12));
					}
					break;
				default:
					Console.WriteLine ("command not recognised");
					break;
				} 
			} else if (_mode == 2) {
				if (_buggy == 1) {
					switch (_cmd) {
					case 5:
						Console.WriteLine ("sup: \"buggy 1 stopped at obj \" ");
						b1Obj = true;
						break;
					case 4:
						Console.WriteLine ("sup: \"buggy 1 object has gone \" ");
						b1Obj = false;
						if (!b1under && Lap1 != 4) {
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
						}
						break;
					case 3:
						Console.WriteLine ("sup: \"buggy 1 at gantry 3 \" ");
						b1under = true;
						if (Lap1 < 4 && LocB2 != 3) {
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							LocB1 = 3;
							b1under = false;

							if (LocB2 == 2 && !b2Obj && b2under) {
								if (Lap2 == 3) {
									_serialPort.WriteLine (string.Format ("B{0}S{1}c{2}{3}", 2, sup_ID, park11, park12));
									_serialPort.WriteLine (string.Format ("B{0}S{1}c{2}{3}", 2, sup_ID, park11, park12));
									LocB2 = 0;
									b2under = false;
								} else if (Lap2 < 3) {
									_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 2, sup_ID, go1, go2));
									_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 2, sup_ID, go1, go2));
									LocB2 = 1;
									b2under = false;
								}
							}
						}
						break;
					case 2:
						Console.WriteLine ("sup: \"buggy 1 at gantry 2 \" ");
						b1under = true;
						if (Lap1 == 1 /* && !b1Obj */) {
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 2, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 2, sup_ID, go1, go2));
							b1under = false;
							LocB1 = 2;
							LocB2 = 3;
						} else if (Lap1 < 4 && LocB2 != 2) {
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							b1under = false;
							LocB1 = 2;

							if (LocB2 == 1 && !b2Obj && b2under) {
								_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 2, sup_ID, go1, go2));
								_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 2, sup_ID, go1, go2));
								LocB2 = 3;
								b2under = false;
							}
						}
						break;
					case 1:
						Console.WriteLine ("sup: \"buggy 1 at gantry 1 \" ");
						Lap1++;
						b1under = true;
						if (Lap1 < 4 && LocB2 != 1) {
							Console.WriteLine ("test1");
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							b1under = false;
							LocB1 = 1;

							if (LocB2 == 3 && !b2Obj && b2under) {
								Console.WriteLine("test2");
								_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 2, sup_ID, go1, go2));
								_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 2, sup_ID, go1, go2));
								LocB2 = 2;
								b2under = false;
							}
						} else if (Lap1 == 4) {
							Console.WriteLine ("test3");
							_serialPort.WriteLine (string.Format ("B{0}S{1}c{2}{3}", _buggy, sup_ID, park21, park12));
							_serialPort.WriteLine (string.Format ("B{0}S{1}c{2}{3}", _buggy, sup_ID, park21, park12));
							b1under = false;
							LocB1 = 0;

							if (LocB2 == 3 && !b2Obj && b2under) {
								Console.WriteLine ("test4");
								_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 2, sup_ID, go1, go2));
								_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 2, sup_ID, go1, go2));
								LocB2 = 2;
								b2under = false;
							}
						}
						break;
					default:
						Console.WriteLine ("command not recognised");
						break;
					} 
				} else if (_buggy == 2) {
					//new
					//
					//
					//
					//
					switch (_cmd) {
					case 5:
						Console.WriteLine ("sup: \"buggy 2 stopped at obj \" ");
						b2Obj = true;
						break;
					case 4:
						Console.WriteLine ("sup: \"buggy 2 object has gone \" ");
						b2Obj = false;
						if (!b2under && Lap2 != 3) {
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
						}
						break;
					case 3:
						Console.WriteLine ("sup: \"buggy 2 at gantry 3 \" ");
						b2under = true;
						if (Lap2 < 3 && LocB1 != 3) {
							_serialPort.WriteLine ("Lap2 < 3 && LocB1 != 3");
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							LocB2 = 3;
							b2under = false;

							if (LocB1 == 2 && !b1Obj && b1under) {
								if (Lap1 == 4) {
									_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 1, sup_ID, park11, park12));
									_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 1, sup_ID, park11, park12));
									LocB1 = 0;
									b1under = false;
								} else if (Lap1 < 4) {
									_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 1, sup_ID, go1, go2));
									_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 1, sup_ID, go1, go2));
									LocB1 = 1;
									b1under = false;
								}
							}
						}
						break;
					case 2:
						Console.WriteLine ("sup: \"buggy 2 at gantry 2 \" ");
						b2under = true;
//						if (Lap2 == 1 /* && !b1Obj */ ) {
//							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
//							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 2, sup_ID, go1, go2));
//							b1under = false;
//							LocB1 = 2;
//							LocB2 = 3;
//						}
						if (Lap2 < 3 && LocB1 != 2) {
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							b2under = false;
							LocB2 = 2;

							if (LocB1 == 1 && !b1Obj && b1under) {
								_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 1, sup_ID, go1, go2));
								_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 1, sup_ID, go1, go2));
								LocB1 = 3;
								b1under = false;
							}
						}
						break;
					case 1:
						Console.WriteLine ("sup: \"buggy 2 at gantry 1 \" ");
						Lap2++;
						b2under = true;
						if (Lap2 < 3 && LocB1 != 1) {
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							b2under = false;
							LocB2 = 1;

							if (LocB1 == 3 && !b1Obj && b1under) {
								_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 1, sup_ID, go1, go2));
								_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 1, sup_ID, go1, go2));
								LocB1 = 2;
								b1under = false;
							}
						} else if (Lap2 == 3) {
							_serialPort.WriteLine (string.Format ("B{0}S{1}c{2}{3}", _buggy, sup_ID, park21, park22));
							_serialPort.WriteLine (string.Format ("B{0}S{1}c{2}{3}", _buggy, sup_ID, park21, park22));
							b2under = false;
							LocB2 = 0;

							if (LocB1 == 3 && !b1Obj && b1under) {
								_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 1, sup_ID, go1, go2));
								_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 1, sup_ID, go1, go2));
								LocB1 = 2;
								b1under = false;
							}
						}
						break;
					default:
						Console.WriteLine ("command not recognised");
						break;
					} 
				} 
			}
//			else if (_mode == 3) 
			else if (_mode == 3) {
				if (_buggy == 1) {
					switch (_cmd) {
					case 5:
						Console.WriteLine ("sup: \"buggy 1 stopped at obj \" ");
						b1Obj = true;
						if (Lap1 == 4) {
							_serialPort.WriteLine (string.Format ("B{0}S{1}c{2}{3}", _buggy, sup_ID, turn1, turn2));
							//_serialPort.WriteLine (string.Format ("B{0}S{1}c{2}{3}", _buggy, sup_ID, turn1, turn2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}c{2}{3}", 2, sup_ID, turn1, turn2));
							//_serialPort.WriteLine (string.Format ("B{0}S{1}c{2}{3}", 2, sup_ID, turn1, turn2));

							Lap1++;
							Lap2++;

							_serialPort.WriteLine (string.Format ("B{0}S{1}c{2}{3}", _buggy, sup_ID, following1, following2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}c{2}{3}", _buggy, sup_ID, following1, following2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}c{2}{3}", 2, sup_ID, following1, following2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}c{2}{3}", 2, sup_ID, following1, following2));
						
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 2, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 2, sup_ID, go1, go2));

						}
						break;
					case 4:
						Console.WriteLine ("sup: \"buggy 1 object has gone \" ");
						b1Obj = false;
						if (!b1under && Lap1 != 4) {
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
						}
						break;
					case 3:
						Console.WriteLine ("sup: \"buggy 1 at gantry 3 \" ");
						b1under = true;
						if (Lap1 < 4 && LocB2 != 3) {
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							LocB1 = 3;
							b1under = false;

							if (LocB2 == 2 && !b2Obj && b2under)//this tells the other buggy to go if its waiting
							{
								if (Lap2 == 3) {
									_serialPort.WriteLine (string.Format ("B{0}S{1}c{2}{3}", 2, sup_ID, park11, park12));
									_serialPort.WriteLine (string.Format ("B{0}S{1}c{2}{3}", 2, sup_ID, park11, park12));
									LocB2 = 0;
									b2under = false;
								} else if (Lap2 < 3) {
									_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 2, sup_ID, go1, go2));
									_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 2, sup_ID, go1, go2));
									LocB2 = 1;
									b2under = false;
								}
							}
						}
						//make sure that lap 4 is incremented off after parking
						if (Lap1 > 4 && Lap1 < 9) {
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
						}
						break;
					case 2:
						Console.WriteLine ("sup: \"buggy 1 at gantry 2 \" ");
						b1under = true;
						if (Lap1 == 1 /* && !b1Obj */) {
							b2under = false;
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 2, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 2, sup_ID, go1, go2));
							b1under = false;
							LocB1 = 2;
							LocB2 = 3;
						} else if (Lap1 < 4 && LocB2 != 2) {
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							b1under = false;
							LocB1 = 2;

							if (LocB2 == 1 && !b2Obj && b2under) {
								_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 2, sup_ID, go1, go2));
								_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 2, sup_ID, go1, go2));
								LocB2 = 3;
								b2under = false;
							}
						}
						if (Lap1 > 4 && Lap1 < 8) {
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
						}
						if (Lap1 == 8) {
							_serialPort.WriteLine (string.Format ("B{0}S{1}c{2}{3}", _buggy, sup_ID, park112, park122));
							_serialPort.WriteLine (string.Format ("B{0}S{1}c{2}{3}", _buggy, sup_ID, park112, park122));
						}
						break;
					case 1:
						Console.WriteLine ("sup: \"buggy 1 at gantry 1 \" ");
						Lap1++;
						b1under = true;
						if (Lap1 < 4 && LocB2 != 1) {
							Console.WriteLine ("test1");
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							b1under = false;
							LocB1 = 1;

							if (LocB2 == 3 && !b2Obj && b2under) {
								Console.WriteLine("test2");
								_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 2, sup_ID, go1, go2));
								_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 2, sup_ID, go1, go2));
								LocB2 = 2;
								b2under = false;
							}
						} else if (Lap1 == 4) {
							Console.WriteLine ("test3");
							_serialPort.WriteLine (string.Format ("B{0}S{1}c{2}{3}", _buggy, sup_ID, park21, park12));
							_serialPort.WriteLine (string.Format ("B{0}S{1}c{2}{3}", _buggy, sup_ID, park21, park12));
							b1under = false;
							LocB1 = 0;

							if (LocB2 == 3 && !b2Obj && b2under) {
								Console.WriteLine ("test4");
								_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 2, sup_ID, go1, go2));
								_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 2, sup_ID, go1, go2));
								LocB2 = 2;
								b2under = false;
							}
						}
						if (Lap1 > 4 && Lap1 < 9) {
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
						}
						break;
					default:
						Console.WriteLine ("command not recognised");
						break;
					} 
				} else if (_buggy == 2) {
					//new
					//
					//
					//
					//
					switch (_cmd) {
					case 5:
						Console.WriteLine ("sup: \"buggy 2 stopped at obj \" ");
						b2Obj = true;
						break;
					case 4:
						Console.WriteLine ("sup: \"buggy 2 object has gone \" ");
						b2Obj = false;
						if (!b2under && Lap2 != 3) {
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
						}
						break;
					case 3:
						Console.WriteLine ("sup: \"buggy 2 at gantry 3 \" ");
						b2under = true;
						if (Lap2 < 3 && LocB1 != 3) {
							_serialPort.WriteLine ("Lap2 < 3 && LocB1 != 3");
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							LocB2 = 3;
							b2under = false;

							if (LocB1 == 2 && !b1Obj && b1under) {
								if (Lap1 == 4) {
									_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 1, sup_ID, park11, park12));
									_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 1, sup_ID, park11, park12));
									LocB1 = 0;
									b1under = false;
								} else if (Lap1 < 4) {
									_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 1, sup_ID, go1, go2));
									_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 1, sup_ID, go1, go2));
									LocB1 = 1;
									b1under = false;
								}
							}
						}
						if (Lap2 > 3 && Lap2 < 7) {
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
						}
						break;
					case 2:
						Console.WriteLine ("sup: \"buggy 2 at gantry 2 \" ");
						b2under = true;
						//						if (Lap2 == 1 /* && !b1Obj */ ) {
						//							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
						//							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 2, sup_ID, go1, go2));
						//							b1under = false;
						//							LocB1 = 2;
						//							LocB2 = 3;
						//						}
						if (Lap2 < 3 && LocB1 != 2) {
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							b2under = false;
							LocB2 = 2;

							if (LocB1 == 1 && !b1Obj && b1under) {
								_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 1, sup_ID, go1, go2));
								_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 1, sup_ID, go1, go2));
								LocB1 = 3;
								b1under = false;
							}
						}
						if (Lap2 > 3 && Lap2 < 6) {
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
						}
						if (Lap2 == 6) {
							_serialPort.WriteLine (string.Format ("B{0}S{1}c{2}{3}", _buggy, sup_ID, park212, park222));
							_serialPort.WriteLine (string.Format ("B{0}S{1}c{2}{3}", _buggy, sup_ID, park212, park222));
						}
						break;
					case 1:
						Console.WriteLine ("sup: \"buggy 2 at gantry 1 \" ");
						Lap2++;
						b2under = true;
						if (Lap2 < 3 && LocB1 != 1) {
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							b2under = false;
							LocB2 = 1;

							if (LocB1 == 3 && !b1Obj && b1under) {
								_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 1, sup_ID, go1, go2));
								_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 1, sup_ID, go1, go2));
								LocB1 = 2;
								b1under = false;
							}
						} else if (Lap2 == 3) {
							_serialPort.WriteLine (string.Format ("B{0}S{1}c{2}{3}", _buggy, sup_ID, park21, park22));
							_serialPort.WriteLine (string.Format ("B{0}S{1}c{2}{3}", _buggy, sup_ID, park21, park22));
							b2under = false;
							LocB2 = 0;

							if (LocB1 == 3 && !b1Obj && b1under) {
								_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 1, sup_ID, go1, go2));
								_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", 1, sup_ID, go1, go2));
								LocB1 = 2;
								b1under = false;
							}
						}
						if (Lap2 > 3 && Lap2 < 7) {
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
							_serialPort.WriteLine (string.Format ("B{0}S{1}C{2}{3}", _buggy, sup_ID, go1, go2));
						}
						break;
					default:
						Console.WriteLine ("command not recognised");
						break;
					} 
				} 
			}
			else
			{
				Console.WriteLine ("execute cmd error");
			}
		}

		public static string SetPortName(string defaultPortName)
		{
			string portName;

			Console.WriteLine("Available Ports:");
			foreach (string s in SerialPort.GetPortNames())
			{
				
				Console.WriteLine("   {0}", s);
			}

			Console.Write("COM port({0}): ", defaultPortName);
			portName = Console.ReadLine();

			if (portName == "")
			{
				portName = defaultPortName;
			}
			return portName;
		}

		public static int SetPortBaudRate(int defaultPortBaudRate)
		{
			string baudRate;

			Console.Write("Baud Rate({0}): ", defaultPortBaudRate);
			baudRate = Console.ReadLine();

			if (baudRate == "")
			{
				baudRate = defaultPortBaudRate.ToString();
			}

			return int.Parse(baudRate);
		}

		public static Parity SetPortParity(Parity defaultPortParity)
		{
			string parity;

			Console.WriteLine("Available Parity options:");
			foreach (string s in Enum.GetNames(typeof(Parity)))
			{
				Console.WriteLine("   {0}", s);
			}

			Console.Write("Parity({0}):", defaultPortParity.ToString());
			parity = Console.ReadLine();

			if (parity == "")
			{
				parity = defaultPortParity.ToString();
			}

			return (Parity)Enum.Parse(typeof(Parity), parity);
		}

		public static int SetPortDataBits(int defaultPortDataBits)
		{
			string dataBits;

			Console.Write("Data Bits({0}): ", defaultPortDataBits);
			dataBits = Console.ReadLine();

			if (dataBits == "")
			{
				dataBits = defaultPortDataBits.ToString();
			}

			return int.Parse(dataBits);
		}

		public static StopBits SetPortStopBits(StopBits defaultPortStopBits)
		{
			string stopBits;

			Console.WriteLine("Available Stop Bits options:");
			foreach (string s in Enum.GetNames(typeof(StopBits)))
			{
				Console.WriteLine("   {0}", s);
			}

			Console.Write("Stop Bits({0}):", defaultPortStopBits.ToString());
			stopBits = Console.ReadLine();

			if (stopBits == "")
			{
				stopBits = defaultPortStopBits.ToString();
			}

			return (StopBits)Enum.Parse(typeof(StopBits), stopBits);
		}

		public static Handshake SetPortHandshake(Handshake defaultPortHandshake)
		{
			string handshake;

			Console.WriteLine("Available Handshake options:");
			foreach (string s in Enum.GetNames(typeof(Handshake)))
			{
				Console.WriteLine("   {0}", s);
			}

			Console.Write("Handshake({0}):", defaultPortHandshake.ToString());
			handshake = Console.ReadLine();

			if (handshake == "")
			{
				handshake = defaultPortHandshake.ToString();
			}

			return (Handshake)Enum.Parse(typeof(Handshake), handshake);
		}
	}
}