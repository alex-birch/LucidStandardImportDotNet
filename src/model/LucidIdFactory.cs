using System;
using System.Collections.Concurrent;
using System.Text;
using LucidStandardImport.model;

namespace LucidStandardImport
{
    public interface ILucidIdFactory
    {
        public void AssignId(IIdentifiableLucidObject identifiableLucidObject);
    }

    public class LucidIdFactory : ILucidIdFactory
    {
        private int UniqueId = 0;
        private static readonly char[] AllowedCharacters =
            "abcdefghijklmnopqrstuvwxyz0123456789-_.~".ToCharArray();
        private static readonly int Base = AllowedCharacters.Length;
        private readonly ConcurrentDictionary<IIdentifiableLucidObject, string> IdCache = new();
        private readonly object lockObject = new();

        public void AssignId(IIdentifiableLucidObject identifiableLucidObject)
        {
            if (identifiableLucidObject == null)
                throw new ArgumentNullException(nameof(identifiableLucidObject));

            if (!string.IsNullOrEmpty(identifiableLucidObject.Id))
                return;

            // Check if the object already has an assigned ID in the cache
            if (IdCache.TryGetValue(identifiableLucidObject, out string? existingId))
            {
                if (existingId != null)
                    identifiableLucidObject.Id = existingId;
                return;
            }

            lock (lockObject)
            {
                // Only assign a new ID if it doesn't already have one
                if (string.IsNullOrEmpty(identifiableLucidObject.Id))
                {
                    var newId = GenerateId();
                    identifiableLucidObject.Id = newId;
                    IdCache[identifiableLucidObject] = newId; // Cache the ID
                }
            }
        }

        private string GenerateId()
        {
            var idBuilder = new StringBuilder();
            var isFirstCharacter = true;

            while (true)
            {
                var number = UniqueId;
                idBuilder.Clear();

                do
                {
                    int remainder = number % Base;
                    char character = AllowedCharacters[remainder];

                    // Ensure the ID doesn't start with a special character, skip to the next UniqueId
                    if (isFirstCharacter && !char.IsLetterOrDigit(character))
                    {
                        UniqueId++;
                        break;
                    }

                    idBuilder.Insert(0, character);
                    number /= Base;
                    isFirstCharacter = false;
                } while (number > 0);

                // If we successfully built a valid ID, return it
                if (idBuilder.Length > 0)
                {
                    UniqueId++; // Increment UniqueId for the next ID
                    return idBuilder.ToString();
                }
            }
        }
    }
}
