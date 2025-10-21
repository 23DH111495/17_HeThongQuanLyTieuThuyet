using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace WebNovel.Models
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            // Convert to lowercase and replace spaces with hyphens
            text = text.ToLowerInvariant();

            // Remove special characters and keep only alphanumeric, spaces, and hyphens
            text = Regex.Replace(text, @"[^a-z0-9\s-]", "");

            // Replace multiple spaces with single space
            text = Regex.Replace(text, @"\s+", " ").Trim();

            // Replace spaces with hyphens
            text = text.Replace(" ", "-");

            // Remove multiple consecutive hyphens
            text = Regex.Replace(text, @"-+", "-");

            // Remove leading/trailing hyphens
            text = text.Trim('-');

            // Limit length
            if (text.Length > 50)
                text = text.Substring(0, 50).Trim('-');

            return text;
        }

        public static string EnsureUniqueSlug(string baseSlug, Func<string, bool> slugExistsFunc)
        {
            string slug = baseSlug;
            int counter = 1;

            while (slugExistsFunc(slug))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            return slug;
        }
    }
}