using System;
using System.IO;
using System.Diagnostics;


namespace PROJECT102621SelfCheckoutKiosk {
    struct CashBox {
        //Declare new datatype for physical cash tender as its label members
        public int pennies;
        public int nickels;
        public int dimes;
        public int quarters;
        public int halfCoins;
        public int dollarCoins;
        public int oneDollars;
        public int twoDollars;
        public int fiveDollars;
        public int tenDollars;
        public int twentyDollars;
        public int fiftyDollars;
        public int hundredDollars;
    }//end struct 

    struct TransactionID {
        public int ID;
        public DateTime date;
        public decimal changeGiven;
        public string cardVendor;
        public string[] cardInfo;
    }//end struct 

    class Program {
        //GLOBAL VARIABLES
        static CashBox _kioskBank;
        static TransactionID receipt;
        static decimal customerPaid;
        static decimal cashReturned;
        static int attempts;
        static int index;
        static string keyword;
        static bool transactionCompleted;
        static bool lockCashBox; 
        static int cardCount;
        static string lastDigits;
        static decimal totalPaymentDebited; //keeps track of total credit card payment - EXPAND ON LATER****

        static void Main(string[] args) {
            string systemShutDown = "";
            decimal itemsTotal = 0.0m;
            //decimal dispenseCash = 0.0m;
            bool correctPW = false;

            //ASSIGN STARTING CASH AND COINS IN KIOSK REGISTER
            KioskRegister();

            //START PROGRAM 
            do {
                //DISPLAYS SCREEN HEADER
                DisplayHeadersAndPrompts(1);

                //VARIABLES 
                customerPaid = 0.0m;
                attempts = 0;
                keyword = "";
                transactionCompleted = false;
                lockCashBox = false;
                receipt.changeGiven = 0;
                receipt.cardInfo = new string[3];
                cardCount = 0;
                lastDigits = string.Empty;

                //SCAN ITEMS 
                itemsTotal = ScanItemCost(); 

                //SELECT FORM OF PAYMENT AND PAY BALANCE
                if (itemsTotal > 0) {
                    PaymentSelectionMenu(itemsTotal);
                }//end if 

                //PRINTS RECEIPT IF TRANSACTION IS COMPLETED - CASH OR CARD TRANSACTION MUST BE DONE 
                if (transactionCompleted == true && keyword != "CANCELLED") {
                    string logArguments = TransactionReceipt();

                    //Append sales information to log - add function here
                    CallLogProgram(logArguments);
                }//end if 

                //DISPLAYS SALUTATION
                DisplayHeadersAndPrompts(10);

                //RESET PROGRAM - NOTE: Selecting N will shut down the kiosk; '2021' is the password to shut down program                 
                systemShutDown = StringPrompt("Press ENTER begin a new transaction. ");
                if (systemShutDown == "N") {
                    correctPW = KioskShutDown();
                }//end if 
            } while (correctPW == false);

            //DISPLAYS SHUTDOWN SCREEN 
            DisplayHeadersAndPrompts(12);
        }//end main 

        #region ********* 1. KIOSK FUNCTIONS ****************
        static void KioskRegister() {
            //STARTING MONEY SUPPLY
            _kioskBank.pennies = 0; 
            _kioskBank.nickels = 120; //$6 of nickels or 3 rolls (40 nickels each)
            _kioskBank.dimes = 150; //$15 of dimes or 3 rolls (50 dimes each)
            _kioskBank.quarters = 120; //$50 of quarters or 5 rolls (40 quarters each)
            _kioskBank.halfCoins = 0;
            _kioskBank.dollarCoins = 100; //$100 total
            _kioskBank.oneDollars = 200; //$200 total
            _kioskBank.twoDollars = 50; //$50 total
            _kioskBank.fiveDollars = 100; //$500 total
            _kioskBank.tenDollars = 100; //$1000 total
            _kioskBank.twentyDollars = 100; //$2000 total
            _kioskBank.fiftyDollars = 100; //$5000 total 
            _kioskBank.hundredDollars = 20; //$2000 total 
        }//end function 

        static decimal ScanItemCost() {
            //DECLARE VARIABLES
            bool parsedValue = false;
            string scannedItem = "Y";            
            decimal itemCost = 0.0m;
            decimal balance = 0.0m;
            //decimal formattedItemCost = 0.0m;
            int index = 0;            

            //SCAN THE CUSTOMER'S ITEM COST TO THE KIOSK 
            while (scannedItem != "") {
                scannedItem = StringPrompt($"\nScan Item #{index + 1}\t$");
                parsedValue = decimal.TryParse(scannedItem, out itemCost);

                //Option B: Rounds down to nearest 2nd decimal place
                //formattedItemCost = Math.Round(itemCost, 2, MidpointRounding.ToZero); 

                //Valid input - prevent indexing for negative values and non-numeric values 
                if (itemCost < 0.0m || parsedValue == false || (itemCost * 100) % 1 != 0) {
                    if (scannedItem != "") {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("SCAN ERROR");
                        Console.ForegroundColor = ConsoleColor.White;
                    }//end if 
                } else if (itemCost > 0 && parsedValue == true && (itemCost * 100) % 1 == 0) {
                    //Increment item costs and valid items            
                    balance += itemCost;
                    index++;
                } else if(itemCost == 0 && parsedValue == true) { //ADD FUNCTION FOR EMPLOYEE PASSWORD TO AUTHORIZE FREE ITEM 
                    index++;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("BOGO DEAL");
                    Console.ForegroundColor = ConsoleColor.White;
                }//end else if 
            }//end while

            Console.Write($"\nTOTAL\t\t");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"{balance:C}\n");
            Console.ForegroundColor = ConsoleColor.White;

            return balance;
        }//end function 

        static decimal PaymentSelectionMenu(decimal balance) {
            //NOTE: Returning balance - keep track of any remaining balances 
            int paymentSelection = 0;

            //DISPLAYS SCREEN HEADER AND SCREEN SELECTION PROMPT
            DisplayHeadersAndPrompts(2);
            DisplayHeadersAndPrompts(14);

            while (paymentSelection != 1 && paymentSelection != 2 && paymentSelection != 3) {
                paymentSelection = IntPrompt("Enter one of the following numbers to select a payment: ");
            }//end valid input 

            if (paymentSelection == 1) {
                balance = CashPayMenu(balance);
            } else if (paymentSelection == 2) {
                balance = RunCardPaymentScreen(balance);
            } else if (paymentSelection == 3) {
                if (customerPaid != 0) { //12/18/21****
                    cashReturned = CashOutput(-customerPaid);
                    DisplayPaymentResult(customerPaid, "REFUND");
                }//end if 
                keyword = "CANCELLED";

                //Reset values 
                customerPaid = 0;
                return balance = 0;
            }//end else if 

            return balance;
        }//end function 

        static decimal PaymentSelectionMenu2(decimal balance) {
            //WHEN CASH PAYMENT IS UNAVAILABLE 
            int paymentSelection = 0;

            DisplayHeadersAndPrompts(2);
            DisplayHeadersAndPrompts(15);

            while (paymentSelection != 1 && paymentSelection != 2 && paymentSelection != 3) {
                paymentSelection = IntPrompt("Enter one of the following numbers to select a payment: ");
            }//end valid input 

            if (paymentSelection == 1) {
                balance = RunCardPaymentScreen(balance);
            } else if (paymentSelection == 2) {
                if (customerPaid != 0) { 
                    cashReturned = CashOutput(-customerPaid);
                    DisplayPaymentResult(customerPaid, "REFUND");
                }//end if 
                keyword = "CANCELLED";

                //Reset values 
                customerPaid = 0;
                return balance = 0;
            }//end else if 

            return balance;
        }//end function 

        static decimal SwitchPayment(decimal balance) {
            bool splitPay = YesPrompt("\nWould like to use another payment type [y/n]? ");
            if (splitPay == true) {
                balance = PaymentSelectionMenu(balance); //double check if the function will reassign cashBalance's value ********************************
            }//end if 

            return balance;
        }//end function 

        static bool ValidUSD(decimal currency) {
            bool validUSD = currency == .01m || currency == .05m || currency == .1m || currency == .25m || currency == .50m || currency == 1 || currency == 2 || currency == 5 || currency == 10 || currency == 20 || currency == 50 || currency == 100;

            if (validUSD == false) {
                return false; 
            }//end if

            return true;
        }//end function 

        static void DisplayHeadersAndPrompts(int header) {
            switch (header) {
                case 1:
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("_______________________________________________________________\n");
                    Console.WriteLine("                     WELCOME SHOPPER!                        ");
                    Console.WriteLine("_______________________________________________________________\n");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("INSTRUCTIONS: SCAN an item to begin. Press ENTER for payment.");
                    break;
                case 2:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("_______________________________________________________________\n");
                    Console.WriteLine("                      PAYMENT MENU                         ");
                    Console.WriteLine("_______________________________________________________________\n");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case 3:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("_______________________________________________________________\n");
                    Console.WriteLine("                      PAYMENT MENU                         ");
                    Console.WriteLine("_______________________________________________________________\n");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case 4:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("_______________________________________________________________\n");
                    Console.WriteLine("                   CASH PAYMENT SCREEN                      ");
                    Console.WriteLine("_______________________________________________________________\n");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("INSTRUCTIONS: Enter USD values only.");
                    break;
                case 5:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("_______________________________________________________________\n");
                    Console.WriteLine("                      MONEY DISPENSER                         ");
                    Console.WriteLine("_______________________________________________________________\n");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case 6:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("_______________________________________________________________\n");
                    Console.WriteLine("                    CASHBACK REQUEST                         ");
                    Console.WriteLine("_______________________________________________________________\n");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case 7:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("_______________________________________________________________\n");
                    Console.WriteLine("                    CARD PAYMENT SCREEN                        ");
                    Console.WriteLine("_______________________________________________________________\n");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case 9:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("_______________________________________________________________\n");
                    Console.WriteLine("                    RECEIPT TRANSACTION                         ");
                    Console.WriteLine("_______________________________________________________________\n");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case 10:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("_______________________________________________________________\n");
                    Console.WriteLine("   Please be sure to grab your change (if any) and receipt!");
                    Console.WriteLine(" THANK YOU FOR SHOPPING WITH US. WE HOPE TO SEE YOU BACK SOON!");
                    Console.WriteLine("_______________________________________________________________\n");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case 11:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("__________________________________________________________\n");
                    Console.WriteLine("                    SYSTEM SHUTDOWN                       ");
                    Console.WriteLine("___________________________________________________________\n");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case 12:
                    Console.Clear();
                    Console.WriteLine("THE NHS SELF-CHECKOUT KIOSK IS NOW SHUTTING DOWN...");
                    System.Threading.Thread.Sleep(2000);
                    Console.Clear();
                    Console.WriteLine("SYSTEM SHUT DOWN IS COMPLETE");
                    break;
                case 13:
                    Console.WriteLine("How much would you like?\n");
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine("[ $5 ]\t\t[ $10 ]\t\t[ $20 ]\n");
                    Console.WriteLine("[ $50 ]\t\t[ $100 ]\t[ Cancel - $0 ]");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case 14:
                    Console.WriteLine("(1) Cash Payment");
                    Console.WriteLine("(2) Credit/Debit Card Payment");
                    Console.WriteLine("(3) Cancel Transaction\n");
                    break;
                case 15:
                    Console.WriteLine("(1) Credit/Debit Card Payment");
                    Console.WriteLine("(2) Cancel Transaction\n");
                    break;
            }//end switch 
        }//end function 

        static void DisplayPaymentResult(decimal balance, string keyword) {
            if (keyword == "CHANGE" && balance <= 0.0m) { 
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"\nCHANGE ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{balance:C}");
                Console.ForegroundColor = ConsoleColor.White;
            } else if (keyword == "REFUND") {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("\nRETURNED ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{customerPaid:C}");
                Console.ForegroundColor = ConsoleColor.White;
            } else if (keyword == "CASHBACK") {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("\nCASHBACK RETURN ");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{balance:C}");
                Console.ForegroundColor = ConsoleColor.White;
            } else if (keyword == "INVALID VENDOR") {
                Console.WriteLine("Card Vendor: Unidentified");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nINVALID OR UNKNOWN CARD VENDOR.");
                Console.ForegroundColor = ConsoleColor.White;
            } else if (keyword == "INVALID CARD") {
                Console.WriteLine("Card Authentication: FAIL");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nINVALID CARD NUMBER.");
                Console.ForegroundColor = ConsoleColor.White;
            } else if (keyword == "DECLINED") {
                Console.Write("\nCard Transaction Processing... ");
                System.Threading.Thread.Sleep(2000); //Timer for Banking Status
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("BANK HAS DECLINED YOUR CHARGE. ");
                Console.ForegroundColor = ConsoleColor.White;
            } else if (keyword == "ACCEPTED") {
                Console.Write("Card Transaction Processing....");
                System.Threading.Thread.Sleep(2000); //Timer for Banking Status
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("CARD TRANSACTION ACCEPTED");
                Console.ForegroundColor = ConsoleColor.White;
            } else if (keyword == "PARTIAL PAID") {
                Console.Write("Card Transaction Processing....");
                System.Threading.Thread.Sleep(2000); //Timer for Banking Status
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("PARTIAL PAYMENT ACCEPTED");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"\nREMAINING BALANCE: {balance:C}");
            } else if (keyword == "UNREADABLE") {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nCOULD NOT READ CARD. PLEASE TRY AGAIN");
                Console.ForegroundColor = ConsoleColor.White;
            } else if (keyword == "CASHBACK DECLINED") {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nBANK HAS DECLINED REQUEST FOR CASHBACK.");
                Console.ForegroundColor = ConsoleColor.White;
            }//end else if
        }//end function 

        static string TransactionReceipt() {
            receipt.ID++;
            receipt.date = DateTime.Now;
            string date = receipt.date.ToString("d").Replace("/","-");
            string time = receipt.date.ToString("t").Replace(" ","");
            string logTransaction = string.Empty;
            decimal cardPayA = 0;
            decimal cardPayB = 0;
            decimal cardPayC = 0;

            //PRINT RECEIPT TO CONSOLE SCREEN 
            DisplayHeadersAndPrompts(9);
            Console.WriteLine($"Transaction #:\t\t{receipt.ID}");
            Console.WriteLine($"Date/Time:\t\t{receipt.date}");
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine($"Cash Paid:\t\t{customerPaid:C}");
            string[,] logCardInfo = DisplayCardInfo();
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine($"Change Given:\t\t{receipt.changeGiven:C}");

            //Format card payment transactions to string 
            if (cardCount == 1) {
                cardPayA = StringToDec(logCardInfo[0, 1]);
                return logTransaction = $"{receipt.ID} {date} {time} {customerPaid:C} {logCardInfo[0, 0]} {cardPayA:C} null 0 null 0 {receipt.changeGiven:C}";
            } else if (cardCount == 2) {
                cardPayA = StringToDec(logCardInfo[0, 1]);
                cardPayB = StringToDec(logCardInfo[1, 1]);
                return logTransaction = $"{receipt.ID} {date} {time} {customerPaid:C} {logCardInfo[0, 0]} {cardPayA:C} {logCardInfo[1, 0]} {cardPayB:C} null 0 {receipt.changeGiven:C}";
            } else if (cardCount == 3) {
                cardPayA = StringToDec(logCardInfo[0, 1]);
                cardPayB = StringToDec(logCardInfo[1, 1]);
                cardPayC = StringToDec(logCardInfo[2, 1]);
                return logTransaction = $"{receipt.ID} {date} {time} {customerPaid:C} {logCardInfo[0, 0]} {cardPayA:C} {logCardInfo[1, 0]} {cardPayB:C} {logCardInfo[2, 0]} {cardPayC:C} {receipt.changeGiven:C}";
            }//end else 
                                                                                                                                                                                                             
            //Format cash payment only transaction to string 
            return logTransaction = $"{receipt.ID} {date} {time} {customerPaid:C} null 0 null 0 null 0 {receipt.changeGiven:C}";
        }//end function 

        static void StoreCardInfo(string card) {
            receipt.cardInfo[cardCount] = $"{receipt.cardVendor},{card},{lastDigits}";
            cardCount++;
        }//end function 

        static string[,] DisplayCardInfo() {
            //Extract multiple credit card vendor and payment
            string[] cardsUsed = new string[cardCount];
            string[,] logCardInfo = new string[cardCount,2];
            decimal payment = 0;
            decimal totalCardPayment = 0;
            Console.WriteLine("------------------------------------------------");
            Console.WriteLine($"Vendor & Payment:\t\t");

            for (int card = 0; card < cardCount; card++) {
                //Extract each card info 
                cardsUsed = receipt.cardInfo[card].Split(',');

                //Convert string to decimal 
                decimal.TryParse(cardsUsed[1], out payment);

                //ADD FUNCTION THAT GETS LAST FOUR DIGITS OF CC
                Console.WriteLine($"{cardsUsed[0].ToUpper()} ending {cardsUsed[2]}\t{payment:C}");

                //GET TOTAL CARD PAYMENT
                totalCardPayment += payment;

                //ADD VENDOR AND CARD PAYMENTS TO MULTIDIMENSIONAL ARRAY 
                logCardInfo[card,0] = cardsUsed[0].ToString().ToUpper();
                logCardInfo[card, 1] = payment.ToString();
            }//end for
            Console.WriteLine("\t\t\t______");
            Console.WriteLine($"Card total\t\t{totalCardPayment:C}");
            //return totalCardPayment; //decimal 

            //Returns all card info used 
            return logCardInfo; 
        }//end function 

        static void CallLogProgram(string logTransaction) {
            //Start new process 
            ProcessStartInfo startInfo = new ProcessStartInfo();

            //Point to logging program     
            startInfo.FileName = @"C:\Users\12283\source\repos\Changebot Project - LogTransaction\bin\Debug\MyTransactionLog.exe";

            //Feed program info as a string
            startInfo.Arguments = logTransaction;

            //From Andrew's notes
            startInfo.CreateNoWindow = false; 

            //Run program
            Process.Start(startInfo);
        }//end function 

        static bool KioskShutDown() {
            int attempts = 0;
            string kioskPassword = "";

            //DISPLAYS SCREEN HEADER
            DisplayHeadersAndPrompts(11);            

            do {
                kioskPassword = StringPrompt("Enter kiosk password: ");
                attempts++;

                if (kioskPassword == "2021") {
                    return true;
                }//end 
            } while (attempts < 3 && kioskPassword != "2021");

            return false; 
        }//end function 
        #endregion

        #region ********* 2. CASH PAYMENT FUNCTIONS *********
        static decimal CashPayMenu(decimal balance) {
            bool fullPayment = false;
            string cashEntered = "";
            decimal cashOutput = 0.0m;
            decimal cashDispensed = 0.0m;
            bool validUSD = false;

            //DISPLAYS SCREEN HEADER
            DisplayHeadersAndPrompts(4);

            do {
                Console.WriteLine($"\nRemaning Balance {balance:C}");
                cashEntered = StringPrompt($"Payment #{index + 1}: $");

                if (cashEntered == "") {
                    balance = SwitchPayment(balance);
                    //return balance;
                } else if (cashEntered != "" && transactionCompleted == false) {
                    decimal.TryParse(cashEntered, out cashOutput);
                    validUSD = ValidUSD(cashOutput);

                    //Money and payment count 
                    if (cashOutput > 0 && validUSD == true) {
                        customerPaid += cashOutput;
                        balance -= CashInput(cashOutput);
                        index++;
                    } else {
                        //Invalid USD 
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("INVALID CURRRENCY");
                        Console.ForegroundColor = ConsoleColor.White;
                    }//end else     
                }//end if else

                if (balance <= 0.0m) {
                    fullPayment = true;
                }//end if
            } while (balance > 0.0m && fullPayment == false);

            //Displays and dispenses money
            if (balance <= 0 && fullPayment == true && keyword != "CANCELLED") {
                keyword = "CHANGE"; 
                DisplayPaymentResult(balance, "CHANGE");
                
                if (balance < 0.0m) {                    
                    cashDispensed = CashOutput(balance);
                }//end if 

                //Reset values 
                fullPayment = false;
            }//end if 

            //Processes Refund
            if (cashDispensed > 0) {
                //Dispenses refund
                cashDispensed = CashOutput(-customerPaid);
                DisplayPaymentResult(customerPaid, "REFUND");
                balance += customerPaid;
                customerPaid = 0; //reset

                transactionCompleted = false;

                //Offer another option to pay                 
                while (balance > 0 && attempts < 3 && YesPrompt("\nWould you like to pay with a credit card [y/n]? ")) {
                    //balance = RunCardPaymentScreen(balance);
                    balance = PaymentSelectionMenu2(balance);
                    attempts++;
                }//end while 

                //Reset values 
                balance = 0.0m;
                fullPayment = false;
                return balance;
            } else {
                //CASH PAID **** CONSTANT LOOP MAY BE AN ISSUE 
                transactionCompleted = true;
            }//end else if 

            return balance = 0;
        }//end function        

        static decimal CashInput(decimal moneySubmitted) {
            if (moneySubmitted == .01m) {
                _kioskBank.pennies += 1;

            } else if (moneySubmitted == .05m) {
                _kioskBank.nickels += 1;

            } else if (moneySubmitted == .10m) {
                _kioskBank.dimes += 1;

            } else if (moneySubmitted == .25m) {
                _kioskBank.quarters += 1;

            } else if (moneySubmitted == 1) {
                _kioskBank.oneDollars += 1;

            } else if (moneySubmitted == 2) {
                _kioskBank.twoDollars += 1;

            } else if (moneySubmitted == 5) {
                _kioskBank.fiveDollars += 1;

            } else if (moneySubmitted == 10) {
                _kioskBank.tenDollars += 1;

            } else if (moneySubmitted == 20) {
                _kioskBank.twentyDollars += 1;

            } else if (moneySubmitted == 50) {
                _kioskBank.fiftyDollars += 1;

            } else if (moneySubmitted == 100) {
                _kioskBank.hundredDollars += 1;

            }//end if 

            return moneySubmitted;
        }//end function

        static decimal CashOutput(decimal cashReturn) {
            int hundred = 0;
            int fifty = 0;
            int twenty = 0;
            int ten = 0;
            int five = 0;
            int two = 0;
            int one = 0;
            int halfCoin = 0;
            int quarter = 0;
            int dime = 0;
            int nickel = 0;
            int penny = 0;
            decimal usTender = 0.0m;
           
            cashReturn *= -1;

            //Add Round Down 
            //cashReturn = Math.Round(cashReturn, 2);

            #region GREEDY ALGORITHM CALCULATION 
            //Determines what bills and coins to dispense based on kiosk's availability
            if (cashReturn >= 100 && _kioskBank.hundredDollars > (int)cashReturn / 100) {
                hundred = (int)(cashReturn / 100); //Calculates how many bills are needed for the customer's change
                cashReturn = cashReturn % 100; //Calculates remaining change owed to customer
            }//end if 

            if (cashReturn >= 50 && _kioskBank.fiftyDollars > (int)cashReturn / 50) {
                fifty = (int)(cashReturn / 50);
                cashReturn = cashReturn % 50;
            }//end if 

            if (cashReturn >= 20 && _kioskBank.twentyDollars > (int)cashReturn / 20) {
                twenty = (int)(cashReturn / 20);
                cashReturn = cashReturn % 20;
            }//end if 

            if (cashReturn >= 10 && _kioskBank.tenDollars > (int)cashReturn / 10) {
                ten = (int)(cashReturn / 10);
                cashReturn = cashReturn % 10;
            }//end if 

            if (cashReturn >= 5 && _kioskBank.fiveDollars > (int)cashReturn / 5) {
                five = (int)(cashReturn / 5);
                cashReturn = cashReturn % 5;
            }//end if 

            if (cashReturn >= 2 && _kioskBank.twoDollars > (int)cashReturn / 2) {
                two = (int)(cashReturn / 2);
                cashReturn = cashReturn % 2;
            }//end if 

            if (cashReturn >= 1 && _kioskBank.oneDollars > (int)cashReturn / 1) {
                one = (int)(cashReturn / 1);
                cashReturn = cashReturn % 1;
            }//end if 

            if (cashReturn >= .50m && _kioskBank.halfCoins > (int)cashReturn / .50) {
                halfCoin = (int)(cashReturn / .50m);
                cashReturn = cashReturn % .50m;
            }//end if 

            if (cashReturn >= .25m && _kioskBank.quarters > (int)cashReturn / .25) {
                quarter = (int)(cashReturn / .25m);
                cashReturn = cashReturn % .25m;
            }//end if 

            if (cashReturn >= .10m && _kioskBank.dimes > (int)cashReturn / .10) {
                dime = (int)(cashReturn / .10m);
                cashReturn = cashReturn % .10m;
            }//end if 

            if (cashReturn >= .05m && _kioskBank.nickels > (int)cashReturn / .05) {
                nickel = (int)(cashReturn / .05m);
                cashReturn = cashReturn % .05m;
            }//end if 

            if (cashReturn >= .01m && _kioskBank.pennies > (int)cashReturn / .01) {
                penny = (int)(cashReturn / .01m);
                cashReturn = cashReturn % .01m;
            } //end if 
            #endregion

            #region DISPENSES CUSTOMER CASH
            if (cashReturn == 0) {
                //DISPLAYS SCREEN HEADER
                DisplayHeadersAndPrompts(5);

                Console.ForegroundColor = ConsoleColor.Green;
                _kioskBank.hundredDollars -= hundred; //Cash is physically deducted from the kiosk and dispensed as customer change 
                for (usTender = 0; usTender < hundred; usTender++) { //Runs loop and display physical bills dispensed 
                    Console.WriteLine("$100 bill dispensed");
                    ChangeBack(100); //Tracks change/cashback dispensed to customer
                }//end for 

                _kioskBank.fiftyDollars -= fifty;
                for (usTender = 0; usTender < fifty; usTender++) {
                    Console.WriteLine("$50 bill dispensed");
                    ChangeBack(50);
                }//end for 

                _kioskBank.twentyDollars -= twenty;
                for (usTender = 0; usTender < twenty; usTender++) {
                    Console.WriteLine("$20 bill dispensed");
                    ChangeBack(20);
                }//end for 

                _kioskBank.tenDollars -= ten;
                for (usTender = 0; usTender < ten; usTender++) {
                    Console.WriteLine("$10 bill dispensed");
                    ChangeBack(10);
                }//end for 

                _kioskBank.fiveDollars -= five;
                for (usTender = 0; usTender < five; usTender++) {
                    Console.WriteLine("$5 bill dispensed");
                    ChangeBack(5);
                }//end for 

                _kioskBank.twoDollars -= two;
                for (usTender = 0; usTender < two; usTender++) {
                    Console.WriteLine("$2 bill dispensed");
                    ChangeBack(2);
                }//end for 

                if (_kioskBank.oneDollars >= one) {
                    _kioskBank.oneDollars -= one;
                    for (usTender = 0; usTender < one; usTender++) {
                        Console.WriteLine("$1 bill dispensed");
                        ChangeBack(1);
                    }//end for 
                } else if (_kioskBank.dollarCoins >= one && _kioskBank.oneDollars < one) {
                    _kioskBank.dollarCoins -= one;
                    for (usTender = 0; usTender < one; usTender++) {
                        Console.WriteLine("$1 coin dispensed");
                        ChangeBack(1);
                    }//end for  
                }//end if 

                _kioskBank.halfCoins -= halfCoin;
                for (usTender = 0; usTender < halfCoin; usTender++) {
                    Console.WriteLine("$0.50 coin dispensed");
                    ChangeBack(.50m);
                }//end for 

                _kioskBank.quarters -= quarter;
                for (usTender = 0; usTender < quarter; usTender++) {
                    Console.WriteLine("$0.25 coin dispensed");
                    ChangeBack(.25m);
                }//end for 

                _kioskBank.dimes -= dime;
                for (usTender = 0; usTender < dime; usTender++) {
                    Console.WriteLine("$0.10 coin dispensed");
                    ChangeBack(.10m);
                }//end for 

                _kioskBank.nickels -= nickel;
                for (usTender = 0; usTender < nickel; usTender++) {
                    Console.WriteLine("$0.05 coin dispensed");
                    ChangeBack(.05m);
                }//end for 

                _kioskBank.pennies -= penny;
                for (usTender = 0; usTender < penny; usTender++) {
                    Console.WriteLine("$0.01 coin dispensed");
                    ChangeBack(.01m);
                }//end for 
            } else if (cashReturn > 0) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n\tWE APOLOGIZE FOR THE INCONVENIENCE.\nTHE KIOSK IS LOW ON FUNDS TO COMPLETE TRANSACTION.\nPLEASE CONTACT A STORE ASSOCIATE FOR FURTHER ASSISTANCE.");
                receipt.changeGiven = 0;
                keyword = "REFUND";
                lockCashBox = true; //Keeps customer from accessing cash option after receiving insufficient funds notice 
            }//end if 
            Console.ForegroundColor = ConsoleColor.White;
            #endregion

            return cashReturn;
        }//end function

        static void ChangeBack(decimal changeGiven) {
            if (keyword == "CHANGE" || keyword == "CASHBACK") {
                receipt.changeGiven += changeGiven;
            }//end if 
        }//end function 

        #endregion

        #region ********* 3. CARD PAYMENT FUNCTIONS *********
        static void GetLastFourDigits(string cardNumbers) {  
            //Retrieves the last 4 digits of a card and display it in the receipt section
            char[] splitCharNum = cardNumbers.ToCharArray();
            int[] splitIntNum = new int[splitCharNum.Length];

            for (int index = 0; index < splitCharNum.Length; index++) {
                //Convert each char to int
                splitIntNum[index] = (int)splitCharNum[index];

                //Convert hex to char value 
                splitIntNum[index] = splitIntNum[index] - 48;
            }//end for 

            switch (splitIntNum.Length) {
                case 13:
                    //Convert int to string
                    lastDigits = $"{splitIntNum[9]}{splitIntNum[10]}{splitIntNum[11]}{splitIntNum[12]}";
                    break;
                case 15:
                    lastDigits = $"{splitIntNum[11]}{splitIntNum[12]}{splitIntNum[13]}{splitIntNum[14]}";
                    break;
                case 16:
                    lastDigits = $"{splitIntNum[12]}{splitIntNum[13]}{splitIntNum[14]}{splitIntNum[15]}";
                    break;
            }//end switch
        }//end function 

        static decimal RoundCurrency(decimal currency) {
            //Round down nearest hundredth decimal place - Mainly used for return from Money Request 
            currency = Math.Round(currency, 2);

            return currency;
        }//end function 

        static decimal RunCardPaymentScreen(decimal balance) {
            decimal dispenseCashBack = 0;
            decimal chargeAccepted = 0;
            decimal dispensedCash = 0.0m;
            string cardNumbers = "";
            string[] chargeApproval;
            string[] cashBackApproval;
            bool isValidCard = false;

            //Cashback Request - Can only be requested once
            if (dispenseCashBack == 0) {
                dispenseCashBack = CashBackRequest();
            }//end if 

            //Swipe Card
            cardNumbers = StringPrompt("Enter your credit/debit card number: ").Replace("-", "");

            if (cardNumbers != "") {
                //Get last 4 digits of cc
                GetLastFourDigits(cardNumbers);

                //1st Step: Identify Vendor
                cardNumbers = FindCardVendor(cardNumbers);
                if (cardNumbers != "INVALID VENDOR") {
                    //2nd Step: Identify Card Number
                    isValidCard = ValidateCardNumber(cardNumbers);
                    if (isValidCard == true) {
                        //3rd Step: Accept/Decline Charge and Cashback
                        chargeApproval = MoneyRequest(cardNumbers, balance);
                        cashBackApproval = MoneyRequest(cardNumbers, dispenseCashBack);

                        if (chargeApproval[1] != "declined") {
                            //Convert string[] to decimal 
                            decimal.TryParse(chargeApproval[1], out chargeAccepted);

                            //Round currency 
                            chargeAccepted = RoundCurrency(chargeAccepted);

                            //FULL CHARGE ACCEPTED
                            if (chargeAccepted == balance) {
                                DisplayPaymentResult(balance, "ACCEPTED");
                                balance -= chargeAccepted;
                                Console.WriteLine($"\nREMAINING BALANCE: {balance:C}");
                                StoreCardInfo(chargeApproval[1].ToString());
                                transactionCompleted = true;
                            } else if (balance > chargeAccepted) {
                                //PARTIAL PAYMENT ACCEPTED
                                balance -= chargeAccepted;
                                DisplayPaymentResult(balance, "PARTIAL PAID");
                                StoreCardInfo(chargeAccepted.ToString());//*****
                                balance = PaymentAttempts(balance);
                                if (customerPaid > 0 && keyword == "CANCELLED") {
                                    //
                                    dispensedCash = CashOutput(-customerPaid);
                                    customerPaid = 0;
                                }//end 4th tier if
                                return balance;
                            }//end 5th valid if 

                            //DISPENSE CASHBACK
                            decimal.TryParse(cashBackApproval[1], out chargeAccepted);
                            chargeAccepted = RoundCurrency(chargeAccepted);
                            if (dispenseCashBack > 0 && balance == 0 && chargeAccepted == dispenseCashBack && chargeAccepted > 0) {
                                //Cashback is only dispensed when request is fully accepted
                                DisplayPaymentResult(chargeAccepted, "CASHBACK");
                                keyword = "CASHBACK";
                                dispensedCash = CashOutput(-chargeAccepted); //**** switch from dispenseCashback to chargeAccepted
                                transactionCompleted = true; 
                            } else if (dispenseCashBack > 0) {
                                DisplayPaymentResult( 0.0m, "CASHBACK DECLINED");
                                dispenseCashBack = 0; //resets cashback request for new card  
                            }//end 5th if  

                        } else if (chargeApproval[1] == "declined") {
                            //CHARGE DECLINED
                            DisplayPaymentResult(balance, "DECLINED");
                            balance = PaymentAttempts(balance);
                            if (customerPaid > 0) {
                                keyword = "CANCELLED";
                                dispensedCash = CashOutput(-customerPaid);
                                //Reset values 
                                customerPaid = 0;
                            }//end nested if 
                            return balance;
                        }//end 4th valid if  

                    } else if (isValidCard == false) {
                        //INVALID CARD NUMBER
                        DisplayPaymentResult(balance, "INVALID CARD");
                        balance = PaymentAttempts(balance);
                        if (customerPaid > 0) {
                            keyword = "CANCELLED";
                            dispensedCash = CashOutput(-customerPaid);
                            //Reset values
                            customerPaid = 0;
                        }//end if 
                        return balance;
                    }//end 3rd valid if 

                } else if (cardNumbers == "INVALID VENDOR") {
                    //INVALID VENDOR
                    DisplayPaymentResult(balance, "INVALID VENDOR");
                    balance = PaymentAttempts(balance);
                    if (customerPaid > 0) {
                        keyword = "CANCELLED";
                        dispensedCash = CashOutput(-customerPaid);
                        //Reset values
                        customerPaid = 0;
                    }//end if 
                    return balance;
                }//end 2nd valid if 

            } else {
                //CARD UNREADABLE
                DisplayPaymentResult( balance , "UNREADABLE");
                balance = PaymentAttempts(balance);
                if (customerPaid > 0) {
                    keyword = "CANCELLED";
                    dispensedCash = CashOutput(-customerPaid);
                    //Reset values 
                    customerPaid = 0;
                    keyword = string.Empty;
                }//end if 
                return balance;
            }//end 1st valid if 

            return balance = 0;
        }//end function 

        static decimal PaymentAttempts(decimal balance) {
            while (balance > 0 && attempts < 2 && YesPrompt("\nWould you like to select another payment [y/n]? ")) {
                attempts++;
                if (lockCashBox == false) {
                    balance = PaymentSelectionMenu(balance);
                } else if (lockCashBox == true) {
                    balance = PaymentSelectionMenu2(balance);
                }//end if
            }//end while 

            if (attempts >= 2 && balance > 0) {
                keyword = "CANCELLED";//12/18/21***
                Console.WriteLine("CREDIT/DEBIT CARD ATTEMPTS HAVE BEEN EXCEEDED.");
                //Add totalPaymentDebited -= totalPaymentDebited;
            }//end if 

            if (balance > 0) {
                keyword = "CANCELLED";//12/18/21***
                Console.WriteLine("TRANSACTION HAS BEEN CANCELLED.");
                //Add totalPaymentDebited -= totalPaymentDebited;
            }//end if 

            return balance = 0;
        }//end function

        static decimal CashBackRequest() {
            //DISPLAYS SCREEN HEADER
            DisplayHeadersAndPrompts(6);

            string cashBackInput = "";
            decimal cashBackOutput = 0;
            bool isParsed = false;
            bool cashBackReq = false;

            cashBackReq = YesPrompt("Would you like to request cashback [y/n]? ");
            if (cashBackReq == true) {
                do {
                    //Displays cashback prompt
                    DisplayHeadersAndPrompts(13);

                    cashBackInput = StringPrompt("\nEnter your amount here: $");
                    Console.WriteLine();
                    isParsed = decimal.TryParse(cashBackInput, out cashBackOutput);
                } while (cashBackOutput != 5 && cashBackOutput != 10 && cashBackOutput != 20 && cashBackOutput != 50 && cashBackOutput != 100 && cashBackOutput != 0);
            }//end if 
            return cashBackOutput;
        }//end function 

        static string FindCardVendor(string cardNumbers) {
            //DISPLAYS SCREEN HEADER
            DisplayHeadersAndPrompts(7);

            string stopCreditCardProcess = "INVALID VENDOR";
            string[] stringEachNumber = new string[cardNumbers.Length];
            int[] intCreditCard = new int[cardNumbers.Length];
            int cardIndex = 0;

            for (cardIndex = 0; cardIndex < cardNumbers.Length; cardIndex++) {
                //Assign each char value to the string array 
                stringEachNumber[cardIndex] = cardNumbers[cardIndex].ToString();

                //Convert string value to int value 
                int.TryParse(stringEachNumber[cardIndex], out intCreditCard[cardIndex]);
            }//end for 

            if (intCreditCard[0] == 3 && intCreditCard.Length == 15) {
                //Checks for AMEX 
                if (intCreditCard[1] == 4 || intCreditCard[1] == 7) {
                    receipt.cardVendor = "AMEX";
                    Console.WriteLine($"Card Vendor: {receipt.cardVendor} {lastDigits}");
                    return cardNumbers;
                } //end nested if 
            }//end amex if 

            if (intCreditCard[0] == 4) {
                //Checks for Visa 
                if (intCreditCard.Length == 13 || intCreditCard.Length == 16) {
                    receipt.cardVendor = "Visa";
                    Console.WriteLine($"Card Vendor: {receipt.cardVendor} {lastDigits}");
                    return cardNumbers;
                }//end nested if 
            }//end visa if  

            if (intCreditCard[0] == 5 && intCreditCard.Length == 16) {
                //Checks for MasterCard 
                if (intCreditCard[1] >= 1 && intCreditCard[1] <= 6) {
                    receipt.cardVendor = "MC";
                    Console.WriteLine($"Card Vendor: {receipt.cardVendor} {lastDigits}");
                    return cardNumbers;
                }//end nested if
            }//end mc if

            if (intCreditCard[0] == 6 && intCreditCard.Length == 16) {
                //Checks for Discover 
                receipt.cardVendor = "DS";
                Console.WriteLine($"Card Vendor: {receipt.cardVendor} {lastDigits}");
                return cardNumbers;
            }//end discover if 

            return stopCreditCardProcess;
        }//end function

        static bool ValidateCardNumber(string acctNumber) {
            int[] intCardPmt = new int[acctNumber.Length]; //Length of cardSwipe var is length of string

            //Converts input to int
            for (int index = 0; index < acctNumber.Length; index++) {
                intCardPmt[index] = (int)(acctNumber[index] - '0'); //Converts char to int from ascii values 
            }//end for 

            //LUHN ALGORITHM: Starting from the right/END, DOUBLE the last digit and each other digit. If greater than 9, mod 10 and +1 to remainder 
            for (int index = intCardPmt.Length - 2; index >= 0; index = index - 2) {
                int tempValue = intCardPmt[index];
                tempValue = tempValue * 2;

                //Mod 10
                if (tempValue > 9) {
                    tempValue = tempValue % 10 + 1;
                }//end if 
                intCardPmt[index] = tempValue;
            }//end for 

            //Add up all digits 
            int sumTotal = 0;
            for (int index = 0; index < intCardPmt.Length; index++) {
                sumTotal += intCardPmt[index];
            }//end for 

            //If number is multiple of 10, it is valid 
            if (sumTotal % 10 == 0) {
                Console.WriteLine("Card Authentication: PASS");
                return true;
            }//end if           

            return false;
        }//end function

        static string[] MoneyRequest(string account_number, decimal amount) {
            Random rnd = new Random();
            //50% CHANCE TRANSACTION PASSES OR FAILS
            bool pass = rnd.Next(100) < 50;
            //50% CHANCE THAT A FAILED TRANSACTION IS DECLINED
            bool declined = rnd.Next(100) < 50;
            if (pass) {
                return new string[] { account_number, amount.ToString() };
            } else {
                if (!declined) {
              return new string[] { account_number, (amount / rnd.Next(2, 6)).ToString() };
                } else {
                  return new string[] { account_number, "declined" };
                }//end if
            }//end if
        }//end function
        #endregion

        #region ********* 4. HELPER FUNCTIONS ***************
        static decimal StringToDec(string input) {
            decimal output = 0;
            decimal.TryParse(input, out output);
            return output;
        }//end function 

        static string StringPrompt(string strInput) {
            //STRING INPUT - Creates a string prompt that returns a string 
            Console.Write(strInput);
            return Console.ReadLine().Trim().ToUpper().Replace(" ", "");
        }//end function 

        static int IntPrompt(string stringMessage) {
            //INT INPUT - Creates a string prompt that return an int
            string input = "";
            int intMessage = 0;
            bool isParsed = false;

            while (isParsed == false) {
                input = StringPrompt(stringMessage);
                isParsed = int.TryParse(input, out intMessage);
            }//end while valid input

            return intMessage;
        }//end function 

        static bool YesPrompt(string strInput) {
            //CHAR INPUT - Creates a string prompt that returns a char ; YES/NO prompt
            char keyPressed = '\0';

            while (keyPressed != 'y' && keyPressed != 'Y' && keyPressed != 'n' && keyPressed != 'N') {
                Console.Write(strInput);
                keyPressed = Console.ReadKey().KeyChar;
                Console.ReadLine().ToUpper().Trim();
                Console.WriteLine();
            }//end while validation 

            return keyPressed == 'y' || keyPressed == 'Y';
        }//end function 
        #endregion
    }//end program 
}//end namespace
