﻿
using Firebase.Auth;
using Firebase.Storage;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Microsoft.Extensions.Configuration;
using NearExpiredProduct.Service.DTO.Request;
using NearExpiredProduct.Service.DTO.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Twilio.TwiML.Voice;
using Stream = System.IO.Stream;
using Task = System.Threading.Tasks.Task;

namespace NearExpiredProduct.Service.Service
{
    public interface IFileStorageService
    {
        Task<string> UploadFileToDefaultAsync(Stream fileStream, string fileName);
    }
    public class FirebaseStorageService : IFileStorageService
    {
        private readonly IConfiguration _config;
        private String ApiKey;
        private static string Bucket;
        private static string AuthEmail;
        private static string AuthPassword;
        public FirebaseStorageService(IConfiguration config)
        {
            _config = config;
            ApiKey = _config["Firebase:ApiKey"];
            Bucket = _config["Firebase:Bucket"];
            AuthEmail = _config["EmailUserName"];
            AuthPassword = _config["EmailPassword"];
        }

        public async Task<string> UploadFileToDefaultAsync(Stream fileStream, string fileName)
        {
            var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
            var a = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);

            var cancellation = new CancellationTokenSource();
            var task = new FirebaseStorage(Bucket,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                    ThrowOnCancel = true
                }
                ).Child("images").Child(fileName).PutAsync(fileStream, cancellation.Token);
            try
            {
                string link = await task;
                return link;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
       
    }
}
