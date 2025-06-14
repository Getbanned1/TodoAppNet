using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Grpc.Auth;
using Grpc.Core;
using System;

namespace TodoAppNet
{
    public class FirestoreService// : IDisposable
    {
        //private readonly FirestoreDb _firestoreDb;
        //private readonly Channel _channel;

        //public FirestoreDb FirestoreDb => _firestoreDb;

        //public FirestoreService(string projectId, string serviceAccountPath)
        //{
        //    // Загружаем учетные данные сервисного аккаунта из файла JSON
        //    GoogleCredential credential = GoogleCredential.FromFile(serviceAccountPath);

        //    // Создаем gRPC канал с аутентификацией
        //    _channel = new Channel(
        //        FirestoreClient.DefaultEndpoint.Host,
        //        FirestoreClient.DefaultEndpoint.Port,
        //        credential.ToChannelCredentials());

        //    // Создаем Firestore клиент с использованием канала
        //    var client = FirestoreClient.Create(_channel);

        //    // Инициализируем FirestoreDb
        //    _firestoreDb = FirestoreDb.Create(projectId, client);
        //}

        //public void Dispose()
        //{
        //    // Закрываем канал при освобождении объекта
        //    _channel.ShutdownAsync().Wait();
        //}
    }
}
