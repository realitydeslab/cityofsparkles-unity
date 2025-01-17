using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.XR.WSA;

namespace TwitterViz.DataModels
{
    public class Tweets
    {
        [JsonProperty("tweets")]
        public Tweet[] AllTweets;
    }

    public class Tweet : ScriptableObject
    {
        [JsonProperty("id")] 
        public long Id;

        [JsonProperty("text")] 
        public string Text;

        [JsonProperty("clean_text")] 
        public string CleanText;

        [JsonProperty("words")] 
        public string[] Words;

        [JsonProperty("place")]
        public Place Place;

        [JsonProperty("sentiment")]
        public Sentiment Sentiment;

        [JsonProperty("coordinates")]
        public Coordinates Coordinates;

        public Tweet(TwitterDatabase.DBTweet dbTweet)
        {
            Id = dbTweet.id;

            Text = dbTweet.clean_text;
            CleanText = dbTweet.clean_text;

            double polarity = dbTweet.sentiment_polarity;
            if (dbTweet.sentiment_positive > 0 && dbTweet.sentiment_positive > dbTweet.sentiment_negative)
            {
                polarity = dbTweet.sentiment_positive;
            }
            else if (dbTweet.sentiment_negative > 0 && dbTweet.sentiment_negative > dbTweet.sentiment_positive)
            {
                polarity = -dbTweet.sentiment_negative;
            }

            Sentiment = new Sentiment()
            {
                Subjectivity = 0, // 1 - dbTweet.sentiment_neutral,
                Polarity = polarity
            };

            if (!Mathf.Approximately((float) dbTweet.latitude, 0) && !Mathf.Approximately((float) dbTweet.longitude, 0))
            {
                Coordinates = new Coordinates()
                {
                    CoordinatesType = "Point",
                    Data = new double[] {dbTweet.longitude, dbTweet.latitude}
                };
            }

            Place = null;

            // Words
            string stripped = Regex.Replace(CleanText, @"[^\u0000-\u007F]+", string.Empty);
            stripped = Regex.Replace(stripped, @",(\S)", @", $1");

            List<string> wordsList = new List<string>(stripped.Split(' '));
            wordsList.Add("- @" + dbTweet.username + ".");

            DateTime createdAt = DateTime.Parse(dbTweet.created_at);
            wordsList.Add(createdAt.ToString("htt, MMM d, yyyy"));
            Words = wordsList.ToArray();
        }

        public override string ToString()
        {
            string sentimentStr = (Sentiment != null) ? Sentiment.Polarity.ToString() : "?";
            return string.Format("{0}: {1}", sentimentStr, CleanText);
        }
    }

    public class Sentiment : ScriptableObject
    {
        [JsonProperty("polarity")]
        public double Polarity;

        [JsonProperty("subjectivity")]
        public double Subjectivity;
    }

    public class Place : ScriptableObject
    {
        [JsonProperty("place_type")]
        public string PlaceType;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("bounding_box")]
        public BoundingBox BoundingBox;
    }

    public class BoundingBox : ScriptableObject
    {
        [JsonProperty("type")]
        public string BoundingBoxType;

        [JsonProperty("coordinates")]
        public double[][][] Coordinates;
    }

    public class Coordinates : ScriptableObject
    {
        [JsonProperty("type")]
        public string CoordinatesType;

        [JsonProperty("coordinates")]
        public double[] Data;
    }
}
