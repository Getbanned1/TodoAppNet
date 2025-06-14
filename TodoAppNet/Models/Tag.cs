using Google.Cloud.Firestore;

[FirestoreData]
public class Tag
{
    [FirestoreDocumentId]
    public string Id { get; set; }

    [FirestoreProperty("name")]
    public string Name { get; set; }

    [FirestoreProperty("color")]
    public string Color { get; set; }
}
