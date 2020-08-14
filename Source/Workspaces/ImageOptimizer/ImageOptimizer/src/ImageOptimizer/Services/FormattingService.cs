using System;

namespace ImageOptimizer.Services
{
    public class FormattingService
    {
        public string AsReadableSize(long fileSize)
        {
            const int roundingDigits = 2;

            const int kb = 1024;
            const int mb = 1048576;
            const int gb = 1073741824;
            const long tb = 1099511627776;

            string sign;
            string suffix;
            long fileSizeInBytes;
            double dynamicSize;

            if (fileSize < 0)
            {
                sign = "-";
                fileSizeInBytes = fileSize * -1;
            }
            else
            {
                sign = string.Empty;
                fileSizeInBytes = fileSize;
            }

            if (fileSizeInBytes < kb)
            {
                dynamicSize = fileSizeInBytes;
                suffix = "bytes";
            }
            else if (fileSizeInBytes < mb)
            {
                dynamicSize = Math.Round((double)fileSizeInBytes / kb, roundingDigits);
                suffix = "KB";
            }
            else if (fileSizeInBytes < gb)
            {
                dynamicSize = Math.Round((double)fileSizeInBytes / mb, roundingDigits);
                suffix = "MB";
            }
            else if (fileSizeInBytes < tb)
            {
                dynamicSize = Math.Round((double)fileSizeInBytes / gb, roundingDigits);
                suffix = "GB";
            }
            else
            {
                dynamicSize = Math.Round((double)fileSizeInBytes / tb, roundingDigits);
                suffix = "TB";
            }

            return sign + dynamicSize + " " + suffix;
        }

        public string AsReadablePercentage(double percentage)
        {
            if (percentage.Equals(0d))
                return "0.00 %";

            var digits = 2;
            var places = 100;

            while (percentage * places < 0.01)
            {
                places *= 10;
                digits++;
            }

            double readablePercentage = Math.Round(percentage * 100, digits);
            return $"{readablePercentage} %";
        }

        public string AsReadablePercentage(long numerator, long denominator)
        {
            if (denominator == 0)
                return "n/a";

            double percentage = (double)numerator / denominator;
            return AsReadablePercentage(percentage);
        }
    }
}
