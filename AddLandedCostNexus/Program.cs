using System;
using System.Collections.Generic;
using System.Linq;
using Avalara.AvaTax.RestClient;

namespace AddLandedCostNexus
{
    class Program
    {
        static void Main(string[] args)
        {
            try {
                Console.WriteLine("This application adds all LandedCost nexus to a given company.");

                string URI      = GetUserUri();                
                int COMPANY_ID  = GetCompanyId();
                string USER     = GetUsername();
                string PASS     = GetPassword();

                AddLcNexusToCompany(URI, USER, COMPANY_ID, PASS);
            }
            catch (Exception exc) {
                Console.WriteLine(exc.Message);
                Console.WriteLine(exc.HelpLink);                
            }
            finally {
                Console.WriteLine("Press enter to continue...");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Adds all LandedCost nexus to a company.
        /// </summary>
        /// <param name="URI">The URI.</param>
        /// <param name="USER">The user.</param>
        /// <param name="COMPANY_ID">The company identifier.</param>
        /// <param name="PASS">The pass.</param>
        private static void AddLcNexusToCompany(string URI, string USER, int COMPANY_ID, string PASS)
        {
            var client = new AvaTaxClient("lcNexusApp",
                                                        ".1",
                                                        "AvalaraAddLcNexusApp",
                                                        new Uri(URI))
                                    .WithSecurity(USER, PASS);
            var countriesAlreadyAssigned = client.ListNexusByCompany(COMPANY_ID, "nexusTaxTypeGroup EQ 'LandedCost'", "", null, null, "").value;
            var countries = client.ListNexusByCountry("", "nexusTaxTypeGroup EQ 'LandedCost'", null, null, "").value;
            List<NexusModel> countriesToAdd = new List<NexusModel>();

            foreach (var country in countries) {
                if (countriesAlreadyAssigned.Any(cas => cas.country == country.country)) {
                    continue;
                }

                countriesToAdd.Add(country);
            }

            client.CreateNexus(COMPANY_ID, countriesToAdd);
            Console.WriteLine(string.Format("Added {0} LandedCostNexus to company {1}.",
                                            countriesToAdd.Count,
                                            COMPANY_ID));
        }

        private static string GetPassword()
        {
            Console.WriteLine("Enter password:");
            string PASS = ReadPassword();
            return PASS;
        }

        private static int GetCompanyId()
        {
            int COMPANY_ID = 0;
            bool isCompanyId = false;
            string companyIdString;

            while (!isCompanyId) {
                Console.WriteLine("Enter company ID to add LandedCost nexus:");
                companyIdString = Console.ReadLine();
                isCompanyId = int.TryParse(companyIdString, out COMPANY_ID);
                if (!isCompanyId) {
                    Console.WriteLine("Company ID must be an integer.");
                }
            }

            return COMPANY_ID;
        }

        private static string GetUsername()
        {
            Console.WriteLine("Enter username:");
            string USER = Console.ReadLine();
            return USER;
        }

        private static string GetUserUri()
        {
            Console.WriteLine("Enter the REST URI to use:");
            string URI = Console.ReadLine();
            return URI;
        }

        public static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo info = Console.ReadKey(true);
            while (info.Key != ConsoleKey.Enter) {
                if (info.Key != ConsoleKey.Backspace) {
                    Console.Write("*");
                    password += info.KeyChar;
                } else if (info.Key == ConsoleKey.Backspace) {
                    if (!string.IsNullOrEmpty(password)) {
                        // remove one character from the list of password characters
                        password = password.Substring(0, password.Length - 1);
                        // get the location of the cursor
                        int pos = Console.CursorLeft;
                        // move the cursor to the left by one character
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                        // replace it with space
                        Console.Write(" ");
                        // move the cursor to the left by one character again
                        Console.SetCursorPosition(pos - 1, Console.CursorTop);
                    }
                }
                info = Console.ReadKey(true);
            }

            // add a new line because user pressed enter at the end of their password
            Console.WriteLine();
            return password;
        }
    }
}
