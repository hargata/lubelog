using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
try
{
    var response = await client.GetAsync("http://localhost:8080/health");
    return response.IsSuccessStatusCode ? 0 : 1;
}
catch
{
    return 1;
}
