﻿using System;
using Terminals.Configuration;
using Terminals.Security;

namespace Terminals.Data.Credentials
{
    internal class GuardedCredential : IGuardedCredential
    {
        private readonly ICredentialBase credential;

        protected PersistenceSecurity PersistenceSecurity { get; private set; }

        /// <summary>
        /// Gets or sets the user name in not encrypted form. This value isn't stored.
        /// this property needs to be public, because it is required by the validation.
        /// </summary>
        public string UserName
        {
            get
            {
                return this.GetDecryptedUserName();
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    this.credential.EncryptedUserName = String.Empty;
                else
                    this.credential.EncryptedUserName = this.PersistenceSecurity.EncryptPersistencePassword(value);
            }
        }

        public string Domain
        {
            get
            {
                if (!string.IsNullOrEmpty(this.credential.EncryptedDomain))
                    return this.PersistenceSecurity.DecryptPersistencePassword(this.credential.EncryptedDomain);

                return String.Empty;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    this.credential.EncryptedDomain = String.Empty;
                else
                    this.credential.EncryptedDomain = this.PersistenceSecurity.EncryptPersistencePassword(value);
            }
        }

        public string Password { get; set; }

        public string EncryptedPassword { get; set; }

        internal GuardedCredential(ICredentialBase credential, PersistenceSecurity persistenceSecurity)
        {
            this.credential = credential;
            this.PersistenceSecurity = persistenceSecurity;
        }

        /// <summary>
        /// Replaces stored encrypted password by new one using newKeymaterial
        /// </summary>
        /// <param name="newKeymaterial">key created from master password hash</param>
        internal void UpdatePasswordByNewKeyMaterial(string newKeymaterial)
        {
            string userName = this.GetDecryptedUserName();
            if (!string.IsNullOrEmpty(userName))
                this.credential.EncryptedUserName = PasswordFunctions2.EncryptPassword(userName, newKeymaterial);

            string domain = this.GetDecryptedDomain();
            if (!string.IsNullOrEmpty(domain))
                this.credential.EncryptedDomain = PasswordFunctions2.EncryptPassword(domain, newKeymaterial);

            string secret = this.GetDecryptedPassword();
            if (!string.IsNullOrEmpty(secret))
                this.credential.EncryptedPassword = PasswordFunctions2.EncryptPassword(secret, newKeymaterial);
        }

        private string GetDecryptedUserName()
        {
            if (!string.IsNullOrEmpty(this.credential.EncryptedUserName))
                return this.PersistenceSecurity.DecryptPersistencePassword(this.credential.EncryptedUserName);

            return String.Empty;
        }

        private string GetDecryptedDomain()
        {
            if (!string.IsNullOrEmpty(this.credential.EncryptedDomain))
                return this.PersistenceSecurity.DecryptPersistencePassword(this.credential.EncryptedDomain);

            return String.Empty;
        }

        private string GetDecryptedPassword()
        {
            if (!string.IsNullOrEmpty(this.credential.EncryptedPassword))
                return this.PersistenceSecurity.DecryptPersistencePassword(this.credential.EncryptedPassword);

            return String.Empty;
        }

        public static void UpdateFromCredential(ICredentialSet source, ISecurityOptions target, PersistenceSecurity persistenceSecurity)
        {
            if (source != null)
            {
                target.Credential = source.Id;
                var guardedSource = new GuardedCredential(source, persistenceSecurity);
                var guardedTarget = new GuardedCredential(target, persistenceSecurity);
                guardedTarget.Domain = guardedSource.Domain;
                guardedTarget.UserName = guardedSource.UserName;
                // todo is it OK to directly assign unencrypted password and avoid encryption
                target.EncryptedPassword = source.EncryptedPassword;
            }
        }
    }
}