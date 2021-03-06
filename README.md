# OneHttpClient

A simple HTTP client for .NET applications that solves common problems usually not noticed by developers.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Introduction

The native .NET `HttpClient` is flexible and powerful and it's possible to extend its behavior in a number of ways.

**However,** it implements the IDisposable interface -- which is not a problem itself. This suggests 
developers to use the `using` structure in order to release resources after each HTTP request, 
which in fact is a problem if you care about the performance of your application.

Some approaches to fix the bad use of `HttpClient` can lead to other problems that I explain briefly 
on the [wiki](https://github.com/psvaiter/one-http-client/wiki). **OneHttpClient** package solves them all.

## Features

- Send HTTP requests to other services (of course)
- (De)Serialize JSON data with the two most used naming strategies: `camelCase` and `snake_case`
- Act on a per-request basis (defaults never existed huh)
- All methods `async`
- Wraps a single instance of HttpClient to be used across entire application
- Honors DNS changes
- Simplicity over fully-featured
- No need to bother about HttpMessageHandlers
- .NET Standard 2.0

## Why use this package?

To put it simple: Because it's easy. Easy to setup. Easy to use.

Every careful developer with good architecture knowledge will wrap dependencies within application's 
own interfaces, so it can be easily replaced with little maintenance effort. We can do it with 
loggers, serializers, authenticators, event hub providers, etc. It's a good practice.

Great, but how many times have you needed to replace some infrastructure component? Almost never, I bet.
It's always a consequence of _"What if one day I need to ...?"_ question. Usually you're right. Other times 
it's just too much, like in the case of HttpClient.

Most applications need to do basic HTTP requests, a bunch of them, all the time, and that's all.
In the world of RESTful APIs that may be sufficient. So you could (and should) use **OneHttpClient**.

If you need complex scenarios like uploading multi-part content or accepting invalid certificates, 
then there's a lot of frameworks out there that may serve you better. From what I saw, they are very 
similar among them in the way to use.

## Install

The package is available at [NuGet.org](https://www.nuget.org/packages/OneHttpClient/). Follow the installation instructions.

## Usage

- Inject the service interface as a dependency of a class that will make HTTP requests.
- Call the service to make a request.

```csharp
using OneHttpClient;

public class Authenticator
{
    private IHttpService _http;
    private string _url;
        
    publlic Authenticator(IHttpService http, AuthenticatorSettings settings) 
    {
        _http = http;
        _url = $"{settings.BaseUrl}/authenticate";
    }
    
    public async Task<bool> AuthenticateAsync(AuthenticateRequest request)
    {
        var headers = new NameValueCollection();
        
        try
        {
            headers.Add("Request-Id", Guid.NewGuid());
            
            var response = await _http.PostAsync<AuthenticateResponse>(_url, request, headers);            
            if (response?.IsSuccessStatusCode == true)
            {
                // Process response
                return true;
            }
        }
        catch (Exception ex)
        {
            // Log error
        }
        
        return false;
    }
}
```
In the example above the response body will be automatically deserialized to `response.ResponseData` as an instance of `AuthenticateResponse` and will also be available in `response.ResponseBody` as a string. If the non-generic overload was used, the response body would only be available as a string and no deserialization would be attempted.
 
> Do not serialize the request, **OneHttpClient** will do it for you.
