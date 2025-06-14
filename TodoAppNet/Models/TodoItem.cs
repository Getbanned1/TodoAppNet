﻿using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace TodoAppNet
{
    [FirestoreData]
    public class TodoItem
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty("title")]
        public string Title { get; set; }

        [FirestoreProperty("description")]
        public string Description { get; set; }

        [FirestoreProperty("dueDate")]
        public Timestamp? DueDate { get; set; }

        [FirestoreProperty("isCompleted")]
        public bool IsCompleted { get; set; }

        [FirestoreProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [FirestoreProperty("sortOrder")]
        public int SortOrder { get; set; }

        [FirestoreProperty("tagIds")]
        public List<string> TagIds { get; set; } = new List<string>();
        public List<Tag> Tags { get; set; } = new List<Tag>();

        // Методы для работы с тегами
        public void AddTag(Tag tag)
        {
            if (tag == null) return;

            string tagId = tag.Id.ToString(); // Явное преобразование в string
            if (!TagIds.Contains(tagId))
                TagIds.Add(tagId);

            if (!Tags.Exists(t => t.Id.ToString() == tagId))
                Tags.Add(tag);
        }

        public void RemoveTag(Tag tag)
        {
            if (tag == null) return;

            string tagId = tag.Id.ToString(); // Явное преобразование в string
            TagIds.Remove(tagId);
            Tags.RemoveAll(t => t.Id.ToString() == tagId);
        }

        public bool HasTag(string tagId) => TagIds.Contains(tagId);

        public override string ToString() => Title;
    }
}