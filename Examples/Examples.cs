using System;

using System.IO;
using System.Threading;
using BoxView;

namespace Examples
{
    public class Examples
    {
        public const string API_KEY = "YOUR_API_KEY";

        public static BoxViewClient BoxView { get; set; }

        public static Document Document1 { get; set; }

        public static Document Document2 { get; set; }

        public static Session Session1 { get; set; }

        public static Session Session2 { get; set; }

        public static DateTime Start = DateTime.Now;

        public static void Main()
        {
            BoxView = new BoxViewClient(API_KEY);

            Example1();
            Example2();
            Example3();
            Example4();
            Example5();
            Example6();
            Example7();
            Example8();
            Example9();
            Example10();
            Example11();
            Example12();
            Example13();
            Example14();
            Example15();
            Example16();
            Example17();
        }

        /// <summary>
        /// Example #1.
        /// Upload a file. We're uploading a sample file by URL.
        /// </summary>
        public static void Example1()
        {
            Console.WriteLine("Example #1 - Upload sample file by URL.");
            var sampleUrl = "http://crocodoc.github.io/dot-net-box-view/Examples/Files/sample.doc";
            Console.Write("  Uploading... ");

            try
            {
                Document1 = BoxView.Upload(sampleUrl, name: "Sample File");

                Console.WriteLine("success :)");
                Console.WriteLine("  ID is " + Document1.Id + ".");
            }
            catch (BoxViewException e)
            {
                Console.WriteLine("failed :(");
                Console.WriteLine("  Error Code: " + e.Code);
                Console.WriteLine("  Error Message: " + e.Message);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Example #2.
        /// Check the metadata of the file from Example #1.
        /// </summary>
        public static void Example2()
        {
            Console.WriteLine("Example #2 - Check the metadata of the file we just uploaded.");
            Console.Write("  Checking metadata... ");

            try
            {
                var documentDuplicate = BoxView.GetDocument(Document1.Id);

                Console.WriteLine("success :)");
                Console.WriteLine("  File ID is " + documentDuplicate.Id + ".");
                Console.WriteLine("  File status is " + documentDuplicate.Status + ".");
                Console.WriteLine("  File name is " + documentDuplicate.Name + ".");
                Console.WriteLine("  File was created on " + documentDuplicate.CreatedAt + ".");
            }
            catch (BoxViewException e)
            {
                Console.WriteLine("failed :(");
                Console.WriteLine("  Error Code: " + e.Code);
                Console.WriteLine("  Error Message: " + e.Message);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Example #3.
        /// Upload another file. We're uploading a sample .doc file from the local filesystem using all options.
        /// </summary>
        public static void Example3()
        {
            Console.WriteLine("Example #3 - Upload a sample .doc as a file using all options.");

            var file = new FileStream("../../Files/sample.doc", FileMode.Open);
            Console.Write("  Uploading... ");

            try
            {
                Document2 = BoxView.Upload(file,
                    name: "Sample File #2",
                    thumbnails: new string[] { "100x100", "200x200" },
                    nonSvg: true);

                Console.WriteLine("success :)");
                Console.WriteLine("  ID is " + Document2.Id + ".");
            }
            catch (BoxViewException e)
            {
                Console.WriteLine("failed :(");
                Console.WriteLine("  Error Code: " + e.Code);
                Console.WriteLine("  Error Message: " + e.Message);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Example #4.
        /// Check the metadata of the file from Example #3.
        /// </summary>
        public static void Example4()
        {
            Console.WriteLine("Example #4 - Check the metadata of the file we just uploaded.");
            Console.Write("  Checking metadata... ");

            try
            {
                var documentDuplicate = BoxView.GetDocument(Document2.Id);

                Console.WriteLine("success :)");
                Console.WriteLine("  File ID is " + documentDuplicate.Id + ".");
                Console.WriteLine("  File status is " + documentDuplicate.Status + ".");
                Console.WriteLine("  File name is " + documentDuplicate.Name + ".");
                Console.WriteLine("  File was created on " + documentDuplicate.CreatedAt + ".");
            }
            catch (BoxViewException e)
            {
                Console.WriteLine("failed :(");
                Console.WriteLine("  Error Code: " + e.Code);
                Console.WriteLine("  Error Message: " + e.Message);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Example #5.
        /// List the documents we've uploaded since starting these Examples.
        /// </summary>
        public static void Example5()
        {
            Console.WriteLine("Example #5 - List the documents we've uploaded so far.");
            Console.Write("  Listing documents... ");

            try
            {
                var documents = BoxView.FindDocuments(createdAfter: Start);

                var doc1 = documents[1];
                var doc2 = documents[0];

                Console.WriteLine("success :)");
                Console.WriteLine("  File #1 ID is " + doc1.Id + ".");
                Console.WriteLine("  File #1 status is " + doc1.Status + ".");
                Console.WriteLine("  File #1 name is " + doc1.Name + ".");
                Console.WriteLine("  File #1 was created on " + doc1.CreatedAt + ".");
                Console.WriteLine("  File #2 ID is " + doc2.Id + ".");
                Console.WriteLine("  File #2 status is " + doc2.Status + ".");
                Console.WriteLine("  File #2 name is " + doc2.Name + ".");
                Console.WriteLine("  File #2 was created on " + doc2.CreatedAt + ".");
            }
            catch (BoxViewException e)
            {
                Console.WriteLine("failed :(");
                Console.WriteLine("  Error Code: " + e.Code);
                Console.WriteLine("  Error Message: " + e.Message);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Example #6.
        /// Wait ten seconds and check the status of both files.
        /// </summary>
        public static void Example6()
        {
            Console.WriteLine("Example #6 - Wait ten seconds and check the status of both files.");
            Console.Write("  Waiting... ");

            Thread.Sleep(10 * 1000);

            Console.WriteLine("done.");
            Console.Write("  Checking statuses... ");

            try
            {
                var documents = BoxView.FindDocuments(createdAfter: Start);

                Document doc1 = documents[0];
                Document doc2 = documents[1];

                Console.WriteLine("success :)");
                Console.WriteLine("  Status for file #1 (id=" + doc1.Id + ") is " + doc1.Status + ".");
                Console.WriteLine("  Status for file #2 (id=" + doc2.Id + ") is " + doc2.Status + ".");
            }
            catch (BoxViewException e)
            {
                Console.WriteLine("failed :(");
                Console.WriteLine("  Error Code: " + e.Code);
                Console.WriteLine("  Error Message: " + e.Message);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Example #7.
        /// Delete the file we uploaded from Example #1.
        /// </summary>
        public static void Example7()
        {
            Console.WriteLine("Example #7 - Delete the second file we uploaded.");
            Console.Write("  Deleting... ");

            try
            {
                var deleted = Document2.Delete();

                if (deleted)
                {
                    Console.WriteLine("success :)");
                    Console.WriteLine("  File was deleted.");
                }
                else
                {
                    Console.WriteLine("failed :(");
                }
            }
            catch (BoxViewException e)
            {
                Console.WriteLine("failed :(");
                Console.WriteLine("  Error Code: " + e.Code);
                Console.WriteLine("  Error Message: " + e.Message);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Example #8.
        /// Update the name of the file from Example #1.
        /// </summary>
        public static void Example8()
        {
            Console.WriteLine("Example #8 - Update the name of a file.");
            Console.Write("  Updating... ");

            try
            {
                var updated = Document1.Update(name: "Updated Name");

                if (updated)
                {
                    Console.WriteLine("success :)");
                    Console.WriteLine("  File ID is " + Document1.Id + ".");
                    Console.WriteLine("  File status is " + Document1.Status + ".");
                    Console.WriteLine("  File name is " + Document1.Name + ".");
                    Console.WriteLine("  File was created on " + Document1.CreatedAt + ".");
                }
                else
                {
                    Console.WriteLine("failed :(");
                }
            }
            catch (BoxViewException e)
            {
                Console.WriteLine("failed :(");
                Console.WriteLine("  Error Code: " + e.Code);
                Console.WriteLine("  Error Message: " + e.Message);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Example #9.
        /// Download the file we uploaded from Example #1 in its original file format.
        /// </summary>
        public static void Example9()
        {
            Console.WriteLine("Example #9 - Download a file in its original file" + " format.");
            Console.Write("  Downloading... ");

            try
            {
                var contents = Document1.Download();
                var filename = "../../Files/test-original.doc";

                File.WriteAllBytes(filename, contents);

                Console.WriteLine("success :)");
                Console.WriteLine("  File was downloaded to " + filename + ".");
            }
            catch (BoxViewException e)
            {
                Console.WriteLine("failed :(");
                Console.WriteLine("  Error Code: " + e.Code);
                Console.WriteLine("  Error Message: " + e.Message);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Example #10.
        /// Download the file we uploaded from Example #1 as a PDF.
        /// </summary>
        public static void Example10()
        {
            Console.WriteLine("Example #10 - Download a file as a PDF.");
            Console.Write("  Downloading... ");

            try
            {
                var contents = Document1.Download("pdf");
                var filename = "../../Files/test.pdf";

                File.WriteAllBytes(filename, contents);

                Console.WriteLine("success :)");
                Console.WriteLine("  File was downloaded to " + filename + ".");
            }
            catch (BoxViewException e)
            {
                Console.WriteLine("failed :(");
                Console.WriteLine("  Error Code: " + e.Code);
                Console.WriteLine("  Error Message: " + e.Message);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Example #11.
        /// Download the file we uploaded from Example #1 as a zip file.
        /// </summary>
        public static void Example11()
        {
            Console.WriteLine("Example #11 - Download a file as a zip.");
            Console.Write("  Downloading... ");

            try
            {
                var contents = Document1.Download("zip");
                var filename = "../../Files/test.zip";

                File.WriteAllBytes(filename, contents);

                Console.WriteLine("success :)");
                Console.WriteLine("  File was downloaded to " + filename + ".");
            }
            catch (BoxViewException e)
            {
                Console.WriteLine("failed :(");
                Console.WriteLine("  Error Code: " + e.Code);
                Console.WriteLine("  Error Message: " + e.Message);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Example #12.
        /// Download the file we uploaded from Example #1 as a small thumbnail.
        /// </summary>
        public static void Example12()
        {
            Console.WriteLine("Example #12 - Download a small thumbnail from a file.");
            Console.Write("  Downloading... ");

            try
            {
                var contents = Document1.Thumbnail(16, 16);
                var filename = "../../Files/test-thumbnail.png";

                File.WriteAllBytes(filename, contents);

                Console.WriteLine("success :)");
                Console.WriteLine("  File was downloaded to " + filename + ".");
            }
            catch (BoxViewException e)
            {
                Console.WriteLine("failed :(");
                Console.WriteLine("  Error Code: " + e.Code);
                Console.WriteLine("  Error Message: " + e.Message);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Example #13.
        /// Download the file we uploaded from Example #1 as a large thumbnail.
        /// </summary>
        public static void Example13()
        {
            Console.WriteLine("Example #13 - Download a large thumbnail from a file.");
            Console.Write("  Downloading... ");

            try
            {
                var contents = Document1.Thumbnail(250, 250);
                var filename = "../../Files/test-thumbnail-large.png";

                File.WriteAllBytes(filename, contents);

                Console.WriteLine("success :)");
                Console.WriteLine("  File was downloaded to " + filename + ".");
            }
            catch (BoxViewException e)
            {
                Console.WriteLine("failed :(");
                Console.WriteLine("  Error Code: " + e.Code);
                Console.WriteLine("  Error Message: " + e.Message);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Example #14.
        /// Create a session for the file we uploaded from Example #1 with default options.
        /// </summary>
        public static void Example14()
        {
            Console.WriteLine("Example #14 - Create a session for a file with default options.");
            Console.Write("  Creating... ");

            try
            {
                Session1 = Document1.CreateSession();

                Console.WriteLine("success :)");
                Console.WriteLine("  Session id is " + Session1.Id + ".");
                Console.WriteLine("  Session expires on " + Session1.ExpiresAt + ".");
                Console.WriteLine("  Session view URL is " + Session1.ViewUrl + ".");
                Console.WriteLine("  Session assets URL is " + Session1.AssetsUrl + ".");
                Console.WriteLine("  Session realtime URL is " + Session1.RealtimeUrl + ".");
            }
            catch (BoxViewException e)
            {
                Console.WriteLine("failed :(");
                Console.WriteLine("  Error Code: " + e.Code);
                Console.WriteLine("  Error Message: " + e.Message);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Example #15.
        /// Create a session for the file we uploaded from Example #1 all of the options.
        /// </summary>
        public static void Example15()
        {
            Console.WriteLine("Example #15 - Create a session for a file with more of the options.");
            Console.Write("  Creating... ");

            try
            {
                Session2 = Document1.CreateSession(
                    expiresAt: DateTime.Now.AddMinutes(10),
                    isDownloadable: true,
                    isTextSelectable: false
                );

                Console.WriteLine("success :)");
                Console.WriteLine("  Session id is " + Session1.Id + ".");
                Console.WriteLine("  Session expires on " + Session1.ExpiresAt + ".");
                Console.WriteLine("  Session view URL is " + Session1.ViewUrl + ".");
                Console.WriteLine("  Session assets URL is " + Session1.AssetsUrl + ".");
                Console.WriteLine("  Session realtime URL is " + Session1.RealtimeUrl + ".");
            }
            catch (BoxViewException e)
            {
                Console.WriteLine("failed :(");
                Console.WriteLine("  Error Code: " + e.Code);
                Console.WriteLine("  Error Message: " + e.Message);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Example #16.
        /// Delete the two sessions.
        /// </summary>
        public static void Example16()
        {
            Console.WriteLine("Example #16 - Delete the two sessions.");
            Console.Write("  Deleting session #1... ");

            try
            {
                var deleted = Session1.Delete();

                if (deleted)
                {
                    Console.WriteLine("success :)");
                    Console.WriteLine("  Session #1 was deleted.");
                }
                else
                {
                    Console.WriteLine("failed :(");
                }
            }
            catch (BoxViewException e)
            {
                Console.WriteLine("failed :(");
                Console.WriteLine("  Error Code: " + e.Code);
                Console.WriteLine("  Error Message: " + e.Message);
            }

            Console.Write("  Deleting session #2... ");

            try
            {
                var deleted = Session2.Delete();

                if (deleted)
                {
                    Console.WriteLine("success :)");
                    Console.WriteLine("  Session #2 was deleted.");
                }
                else
                {
                    Console.WriteLine("failed :(");
                }
            }
            catch (BoxViewException e)
            {
                Console.WriteLine("failed :(");
                Console.WriteLine("  Error Code: " + e.Code);
                Console.WriteLine("  Error Message: " + e.Message);
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Example #17.
        /// Delete the file we uploaded from Example #1.
        /// </summary>
        public static void Example17()
        {
            Console.WriteLine("Example #17 - Delete the first file we uploaded.");
            Console.Write("  Deleting... ");

            try
            {
                var deleted = Document1.Delete();

                if (deleted)
                {
                    Console.WriteLine("success :)");
                    Console.WriteLine("  File was deleted.");
                }
                else
                {
                    Console.WriteLine("failed :(");
                }
            }
            catch (BoxViewException e)
            {
                Console.WriteLine("failed :(");
                Console.WriteLine("  Error Code: " + e.Code);
                Console.WriteLine("  Error Message: " + e.Message);
            }

            Console.WriteLine();
        }
    }
}

