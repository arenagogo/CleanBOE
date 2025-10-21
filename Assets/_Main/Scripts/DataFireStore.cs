// LoginModels.cs
using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace ArenaGo.Models
{
    [Serializable]
    public class LoginResponse
    {
        public bool success;
        public Data data;
        public string message;
    }

    [Serializable]
    public class Data
    {
        public string uid;
        public string email;
        public bool disabled;
        public bool emailVerified;
        public Metadata metadata;
        public Profile profile;
        public string accessToken;
    }

    [Serializable]
    public class Metadata
    {
        public string creationTime;
        public string lastSignInTime;
    }

    [Serializable]
    public class Profile
    {
        public string id;
        public int totalReferralCoins;
        public string trend;
        public string[] joinedCommunities;
        public int points;
        public string badge;
        public string[] referralBadges;
        public string tier;
        public string phone;
        public string[] joinedCategories;
        public string referredBy;
        public string referralCode;
        public int rank;
        public int voteCount;
        public int badgeCount;
        public int battleWinCount;
        public int crateCount;
        public int battleCount;
        public bool eulaAccepted;
        public string eulaVersion;
        public FirestoreTimestamp eulaAcceptedAt;
        public EquippedAvatar equippedAvatar;
        public int xp;
        public Categories categories;
        public int snapCount;
        public string avatarUrl;
        public FirestoreTimestamp dob;
        public string link;
        public string bio;
        public string username;
        public Companion companion;
        public FirestoreTimestamp updatedAt;
        public FirestoreTimestamp createdAt;
        public FirestoreTimestamp lastActive;
        public string displayName;
        public string gender;
        public string language;
        public string app_version;
        public string platform;
        public FirestoreTimestamp lastSeen;
        public bool isOnline;
        public TrendCache trendCache;
        public string fcm_token;
        public FirestoreTimestamp lastTokenUpdate;
        public int walletCoins;
        public ArenaPassExpire arenaPassExpire;
        public bool arenaPassActive;
        public string discoData;


    }

    public class ArenaPassExpire
    {
        public int seconds;
        public int nanoseconds;
    }

    [Serializable]
    public class EquippedAvatar
    {
        public string gender;
        public string sepatu;
        public string kepala;
        public string celana;
        public string baju;
        public string category;
        public string @base; // pakai escape keyword
        public FirestoreTimestamp updatedAt;
    }

    [Serializable]
    public class Categories
    {
        public CategoryInfo all;
        public CategoryInfo soccer;
        public CategoryInfo motor;
        public CategoryInfo basket;
        public CategoryInfo music_dance;
        public CategoryInfo karma_baik;
        public CategoryInfo padel;
        public CategoryInfo esport;
        public CategoryInfo food;
        public CategoryInfo mobil;
        public CategoryInfo golf;
        public CategoryInfo standup_comedy;
        public CategoryInfo travel;
        public CategoryInfo finance;
        public CategoryInfo musik_dance; // ada 2 di JSON, pastikan konsisten
    }

    [Serializable]
    public class CategoryInfo
    {
        public int points;
        public int level;
        public FirestoreTimestamp lastUpdated;
    }

    [Serializable]
    public class Companion
    {
        public string vibe;
        public string greeting;
        public string name;
        public string avatar;
    }

    [Serializable]
    public class TrendCache
    {
        public string overall;
        public FirestoreTimestamp lastUpdated;
    }

    [Serializable]
    public class FirestoreTimestamp
    {
        public long _seconds;
        public int _nanoseconds;

        public DateTime ToLocalTime()
        {
            var utc = DateTimeOffset.FromUnixTimeSeconds(_seconds).UtcDateTime;
            return utc.AddTicks(_nanoseconds / 100).ToLocalTime();
        }
    }
}

