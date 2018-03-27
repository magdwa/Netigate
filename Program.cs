using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;
using System.IO;

namespace NetiGateAPI
{
    class Program
    {
        static void Main(string[] args)
        {
           
            NetiGate.NetigateAPISoapClient wsClient = new NetiGate.NetigateAPISoapClient();

            //Get GUID & CustomerId
            //Don't store the login & password!!
            System.Xml.Linq.XElement response = wsClient.GetAccountDetails("nilsko@statoilfuelretail.com", "H-74875-P");
            string guid = response.Element("GUID").Value;
            string customerID = response.Element("CustomerId").Value;
            int customerIDint = -1;
            int.TryParse(customerID, out customerIDint);

            //Get SurveyId and GUID for "API Customer Feedback at Station SE"
            response = wsClient.GetSurveyListByCustomerId(customerIDint, guid);
            IEnumerable<XElement> ex1 = response.Descendants("Customer");
            XElement ex2 = response.Element("Customer");
            XElement ex3 = ex2.Element("Surveys");
            IEnumerable<XElement> ex4 = ex3.Elements("Survey");
            IEnumerable<XElement> ex5 = from el in ex4
                 where el.Element("SurveyName").Value == "API Customer Feedback at Station SE"
                 select el;
            string surveyID = ex5.First().Element("SurveyId").Value;
            string strGUID = ex5.First().Element("strGUID").Value;
            int surveyIDint = -1;
            int.TryParse(surveyID, out surveyIDint);

            //Get survey respondents
            response = wsClient.GetAnsweredRespondentList(surveyIDint, strGUID, int.MaxValue, 0);
            IEnumerable<XElement> respondents = response.Descendants("Respondent");

            //get responses & measure execution time
            List<XElement> answers = new List<XElement>();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            foreach (XElement el in respondents)
            {
                response = wsClient.GetAnswersFromRespondent(el.Element("AnswerSetId").Value, surveyIDint, strGUID);
                answers.Add(response);
                
            }
            
            watch.Stop();
            var elapsedSeconds = watch.ElapsedMilliseconds / 1000;

            Console.WriteLine("There were: " + answers.Count + " answers");
            Console.WriteLine("It took: " + elapsedSeconds + " to get the answers");


            //write customers responses to the variable  test

            string csv =
                (from elm in answers.Elements("AnswerSets").Elements("AnswerSet")
                 select
                 string.Format("{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12}\n",


             
             (string)elm.Element("SurveyId"),
             (string)elm.Element("DtmCreated"),
             (string)elm.Element("DtmCompleted"),

             (string)elm.Element("Answers").Element("Answer").Element("AnswerId"),
             (string)elm.Element("Answers").Element("Answer").Element("QNr"),
             (string)elm.Element("Answers").Element("Answer").Element("QId"),
             (string)elm.Element("Answers").Element("Answer").Element("QText"),
             (string)elm.Element("Answers").Element("Answer").Element("QMechanism"),
             (string)elm.Element("Answers").Element("Answer").Element("QMText"),
             (string)elm.Element("Answers").Element("Answer").Element("QType"),
             (string)elm.Element("Answers").Element("Answer").Element("AValue"),
             (string)elm.Element("Answers").Element("Answer").Element("AText"),
             (string)elm.Element("Answers").Element("Answer").Element("AWeight"),

             Environment.NewLine
         )
                )
                .Aggregate(
                    new StringBuilder(),
                    (sb, s) => sb.Append(s),
                    sb => sb.ToString()
                );
            watch.Stop();

            Console.WriteLine(csv);

 


            // write customers response to .csv file
            System.IO.StreamWriter file = new System.IO.StreamWriter("C:\\New folder\\Test.csv");
            file.WriteLine(csv);

            file.Close();

            watch.Stop();


        }
    }
}
