namespace OpenIddict.Sandbox.UnoClient;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Uno;

#if IOS || MACCATALYST
using Apple.CryptoKit;
using Foundation;
#endif
// https://github.com/Ecierge/openiddict-core/blob/apple-cryptokit/sandbox/OpenIddict.Sandbox.Uno.Client/OpenIddictClientCryptographicKeys.cs
internal static class OpenIddictClientCryptographicKeys
{
    internal static RsaSecurityKey GetRsaCngKey(string name, CngKeyUsages usages, CngProvider provider)
    {
        name = name ?? throw new ArgumentNullException(nameof(name));
#if IOS || MACCATALYST

        NSData rawKeyData;

        if (ACKeychainStorage.IsKeyExist(name, false))
        {
            rawKeyData = ACKeychainStorage.GetKey(name, false);
        }
        else
        {
            rawKeyData = ACKeychainStorage.CreateKey(name, "rsa", 2048, false, !(usages == CngKeyUsages.Signing), true);
        }

        int bytesRead = 0;
        byte[] keyData = new byte[rawKeyData.Length];
        Marshal.Copy(rawKeyData.Bytes, keyData, 0, (int)rawKeyData.Length);

        RSA myRsa = RSA.Create();
        myRsa.ImportRSAPrivateKey(keyData, out bytesRead);

        return new RsaSecurityKey(myRsa);
#else
        System.Security.Cryptography.CngKey key;
#pragma warning disable CA2000 // Dispose objects before losing scope
        if (CngKey.Exists(name, provider, CngKeyOpenOptions.UserKey))
        {
            key = CngKey.Open(name, provider, CngKeyOpenOptions.UserKey);
        }

        else
        {
            try
            {
                key = CngKey.Create(CngAlgorithm.Rsa, name, new CngKeyCreationParameters
                {
                    KeyCreationOptions = CngKeyCreationOptions.None,
                    KeyUsage = usages,
                    Parameters = { new CngProperty("Length", BitConverter.GetBytes(2048), CngPropertyOptions.None) },
                    Provider = provider
                });
            }

            // If multiple instances of the application were started at the same time, a race condition
            // might occur here. In this case, try to open the key that was created by the other instance.
            catch (CryptographicException) when (CngKey.Exists(name, provider, CngKeyOpenOptions.UserKey))
            {
                key = CngKey.Open(name, provider, CngKeyOpenOptions.UserKey);
            }
        }

        return new RsaSecurityKey(new RSACng(key));
#pragma warning restore CA2000 // Dispose objects before losing scope
#endif
    }
}
