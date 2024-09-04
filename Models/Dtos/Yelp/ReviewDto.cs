﻿namespace foodies_api.Models.Dtos.Yelp;

public class ReviewDto : User
{
    public string Id { get; set; }
    public string Url { get; set; }
    public string Text { get; set; }
    public int Rating { get; set; }
    public DateTime TimeCreated { get; set; }
    public User User { get; set; }
}

public class User 
{
    public string Id { get; set; }
    public string ProfileUrl { get; set; }
    public string ImageUrl { get; set; }
    public string Name { get; set; }
}