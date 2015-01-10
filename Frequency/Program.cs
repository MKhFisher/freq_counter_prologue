using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Frequency
{
    public class Frequency
    {
        public string term;
        public int frequency;

        public Frequency(string term)
        {
            this.term = term;
            this.frequency = 1;
        }

        public void IncrementFrequency(int increment)
        {
            frequency += increment;
        }
    }

    public class ValidateFrequency
    {
        public string term { get; set; }
        public int frequency { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No text file given to parse.");
            }
            else if (args.Length > 1)
            {
                Console.WriteLine("More than one text file given to parse.");
            }
            else if (!args[0].EndsWith(".txt"))
            {
                Console.WriteLine("The file passed is not a text file.");
            }
            else
            {
                try
                {
                    List<string> stopwords = GetStopWordsFile("https://raw.githubusercontent.com/crista/exercises-in-programming-style/master/stop_words.txt");
                    if (stopwords.Count < 1)
                    {
                        throw new FileNotFoundException();
                    }
                    else
                    {
                        string text = GetTextFile(args[0]);
                        if (text.Length < 1)
                        {
                            throw new FileNotFoundException();
                        }
                        else
                        {
                            List<string> tokens = Tokenize(text);
                            if (tokens.Count < 1)
                            {
                                Console.WriteLine("No words found in input file.");
                            }
                            else
                            {
                                List<Frequency> freq = new List<Frequency>();
                                Hashtable duplicates = new Hashtable();

                                foreach (string word in tokens)
                                {
                                    if (stopwords.Contains(word))
                                    {
                                        continue;
                                    }

                                    Frequency temp = new Frequency(word);
                                    if (duplicates[word] == null)
                                    {
                                        duplicates[word] = temp;
                                    }
                                    else
                                    {
                                        Frequency update = duplicates[word] as Frequency;
                                        update.IncrementFrequency(1);
                                    }
                                }

                                foreach (DictionaryEntry entry in duplicates)
                                {
                                    freq.Add(entry.Value as Frequency);
                                }

                                freq = freq.OrderByDescending(x => x.frequency).ToList();

                                bool IsResultValid = ValidateFrequencies(freq, 25);
                                Console.WriteLine("Frequencies counted and top 25 results validated. Results of validation is: {0}", IsResultValid);
                                for (int i = 0; i < 25; i++)
                                {
                                    Console.WriteLine("{1}  -  {2}", i + 1, freq[i].term, freq[i].frequency);
                                }
                                Console.ReadKey();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    using (StreamWriter sw = new StreamWriter(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\error_log.txt"))
                    {
                        sw.WriteLine("*********************************************");
                        sw.WriteLine(DateTime.Today.ToShortDateString());
                        sw.WriteLine(DateTime.Today.ToShortTimeString());
                        sw.WriteLine(e.ToString());
                        sw.WriteLine("*********************************************");
                    }
                }
            }
        }

        public static bool ValidateFrequencies(List<Frequency> freq, int number_to_validate)
        {
            List<ValidateFrequency> valid_freq = GetValidFreq("https://raw.githubusercontent.com/crista/exercises-in-programming-style/master/test/pride-and-prejudice.txt");
            for (int i = 0; i < number_to_validate; i++)
            {
                var match = valid_freq.Where(x => x.term == freq[i].term && x.frequency == freq[i].frequency);
                if (match == null)
                {
                    return false;
                }
            }

            return true;
        }

        static Regex regex_token = new Regex("(\\w+)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        public static List<string> Tokenize(string input)
        {
            input += ".";
            MatchCollection matches = regex_token.Matches(input);
            List<string> result = new List<string>();

            foreach (Match m in matches)
            {
                if (m.Groups[1].Value.ToLower() != "s")
                {
                    result.Add(m.Groups[1].Value.ToLower());
                }
            }

            return result;
        }

        public static List<ValidateFrequency> GetValidFreq(string location)
        {
            List<ValidateFrequency> result = new List<ValidateFrequency>();

            var request = WebRequest.Create(location);
            using (var response = request.GetResponse())
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] temp = Regex.Split(line, "(\\w+)");
                    result.Add(new ValidateFrequency { term = temp[1], frequency = Int32.Parse(temp[3]) });
                }
            }

            return result;
        }

        public static string GetTextFile(string location)
        {
            var request = WebRequest.Create(location);
            using (var response = request.GetResponse())
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                var words = reader.ReadToEnd();
                //return words.ToString();
                return words.ToString().Replace('_', ' ');
            }
        }

        public static List<string> GetStopWordsFile(string location)
        {
            var request = WebRequest.Create(location);
            using (var response = request.GetResponse())
            using (var content = response.GetResponseStream())
            using (var reader = new StreamReader(content))
            {
                var words = reader.ReadToEnd();
                string[] temp = words.Split(',');

                List<string> result = new List<string>();
                for (int i = 0; i < temp.Length; i++)
                {
                    result.Add(temp[i].Trim());
                }

                return result;
            }
        }
    }
}
