using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LogKioskTransaction {
    class Program {

        static void Main(string[] args) {
            //CHECK TO SEE IF I ONLY HAVE 11 ARGUMENTS
            bool validArguments = args.Length == 11;
            
            if (!validArguments) {
                Console.WriteLine("Error: ELEVEN arguments are needed to completely log a transaction.");
            
                string errorFilepath = Directory.GetCurrentDirectory() + $"\\{args[1]}\terror.txt";
                StreamWriter errorFile = new StreamWriter(errorFilepath);
                errorFile.Write("Error in arguments");
                errorFile.Close();            
                return;
            
            }//end if 

            //Converts information into string format and writes to file
            LogArguments(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7], args[8], args[9], args[10]);
                                  
        }//end main 

        static void LogArguments(string transactionID, string date, string time, string cashPaid, string vendor1, string cardPaid1, string vendor2, string cardPaid2, string vendor3, string cardPaid3, string change) {
            //Converts and compress to string format
            string result = $"Transactions #{transactionID}\nDate: {date}\nTime: {time}\n" +
                    $"Cash Payment: {cashPaid}\nVendor 1: {vendor1}\nCard 1 Payment: {cardPaid1}\nVendor 2: {vendor2}\nCard 2 Payment: {cardPaid2}\n" +
                    $"Vendor 3: {vendor3}\nCard 3 Payment: {cardPaid3}\nChange: {change}\n";

            //Write to new file 
            string filepath = Directory.GetCurrentDirectory() + $"\\{date}\tTransactions.log";
            StreamWriter outfile = new StreamWriter(filepath, true); //appends new entries to existing file names and creates new files with different file names
            outfile.WriteLine(result);
            outfile.Close();
        }//end function 

    }//end program
}//end namespace
