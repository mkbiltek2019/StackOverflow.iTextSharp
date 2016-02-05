﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;

// fit a delimited string into a AcroField
// TODO: single line / check if AcroFields.FIELD_TYPE_TEXT??
namespace kuujinbo.StackOverflow.iTextSharp._scratchSpace
{
    public class FitTextInField
    {
        class DelimetedFieldFitter
        {
            public char Delimeter { get; set; }
            public BaseFont Font { get; set; }
            public float FontSize { get; set; }

            public void FitTextInField(string value, float fieldWidth, out string fits, out string overflow)
            {
                var delimeter = Delimeter.ToString();
                var splitter = new char[] { Delimeter };
                var split = value.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
                var paddedWidth = fieldWidth - Font.GetWidthPoint("0", FontSize) * 2;
                Console.WriteLine("TEST STRING: {0}\n", string.Join(delimeter, split.ToArray()));

                fits = string.Empty;
                overflow = string.Empty;
                int count = 0;
                int start = 0;
                foreach (var element in split)
                {
                    string testString = string.Join(delimeter.ToString(), split.Take(++start).ToArray());
                    var testWidth = Font.GetWidthPoint(testString, FontSize);
                    if (paddedWidth < testWidth)
                    {
                        break;
                    }
                    ++count;
                }
                fits = string.Join(delimeter.ToString(), split.Take(count).ToArray());
                overflow = string.Join(delimeter.ToString(), split.Skip(count).Take(split.Length - count).ToArray());
            }
        }

        public void Go()
        {
            var fileName = "datasheet.pdf";
            var outputFile = Helpers.IO.GetClassOutputPath(this);
            var testField = "title";
            var testText = "0123456789";
            List<string> testTextList = new List<string>();
            for (int i = 1; i <= 20; ++i)
            {
                testTextList.Add(string.Format("{0}[{1}]", testText, i));
            }
            var baseFont = BaseFont.CreateFont();
            float testSize = 5f;
            char delimeter = ',';
            var testJoined = string.Join(delimeter.ToString(), testTextList.ToArray());

            using (var reader = Helpers.IO.GetInputReader(fileName))
            {
                using (var stream = new FileStream(outputFile, FileMode.Create))
                {
                    using (var stamper = new PdfStamper(reader, stream))
                    {
                        AcroFields fields = stamper.AcroFields;
                        var width = fields.GetFieldPositions("title")[0].position.Width;

                        fields.SetFieldProperty(
                            testField, "textfont", baseFont, null
                        );
                        fields.SetFieldProperty(
                            testField, "textsize", testSize, null
                        );

                        var ff = new DelimetedFieldFitter()
                        {
                            Delimeter = delimeter,
                            Font = baseFont,
                            FontSize = testSize
                        };
                        string fit;
                        string over;
                        ff.FitTextInField(testJoined, width, out fit, out over);
                        Console.WriteLine("fit: {0}\n", fit);
                        Console.WriteLine("over: {0}\n", over);

                        fields.SetField(testField, fit);
                    }
                }
            }
        }
    }
}