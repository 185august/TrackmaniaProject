
# Trackmania map times API
This is a local-only API designed to be the backend for a school project to help users compare their Trackmania campaign map times with other players.


## Table of Contents  
* [General info](#Trackmania-map-times-API)
* [Tech Stack](#Tech-Stack)
* [Run Locally](#Run-Locally)
* [API Reference](#API-Reference)
* [Lessons Learned](#Lessons-Learned)
* [Environment Variables](#Enviroment-Variables)
* [Acknowledgements](#Acknowledgements)
## Tech Stack

**Backend:** ASP.NET Core Web API (.NET 9.0) • C# 13.0   
**Database:** MySQL • Entity Framework Core v=9.0.4 • MySQL.EntityFrameworkCore v=9.0.6  
**Documentation:** OpenAPI v=9.0.4 • Scalar.AspNetCore v=2.6.9  
**DevOps:** User Secrets Management  
**External APIs:** Trackmania/Ubisoft APIs



## Run Locally
To run this project, you'll need a local database and a registered OAuth app.

1. Database Setup: The project relies on a local MySQL database. You'll need to create two tables: one for all the campaign maps and another for player profiles.     
A JSON file with the name of CampaignMaps.json containing the map data is included in the project.

2. OAuth App: This project requires you to register an OAuth app to get an Identifier and a Secret.  
Instructions for creating one can be found - [here](https://webservices.openplanet.dev/oauth/auth).



Clone the project

```bash
  git clone https://github.com/185august/TrackmaniaProject.git
```

Go to the project directory

```bash
  cd TrackmaniaProject
```




## API Reference

### Register user

```http
  POST /Auth/Register
```
JSON Body:
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `username` | `string` | **Required**. The chosen username |
| `password` | `string` | **Required**. The chosen password |
| `playerProfile` | `object` | **Required**. An object containing the player's Ubisoft profile data which is retrived from /PlayerAccounts/GetAndUpdatePlayerAccounts endpoint|

Example request:  
``` shell
curl Api/Auth/register \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
  "username": "username",
  "password": "password",
  "playerProfile": {
    "ubisoftUsername": "ubisoftUserId",
    "ubisoftUserId": "longUbisoftId"
  }
}'
```

Example response:
A string indicating if the request was valid.

### Login user

```http
  Post /Auth/login
```
JSON Body:
| Parameter | Type     | Description                       |
| :-------- | :------- | :-------------------------------- |
| `username`      | `string` | **Required**. username of the user |
| `password`      | `string` | **Required**. password of the user |

Example request:  
```shell
curl /Auth/login \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
  "username": "username",
  "password": "password"
}'
```
Example response:
```json
{
  "username": "user1",
  "ubisoftUsername": "user1tm",
  "ubisoftUserId": "longUbisoftId"
}
```
### Get Ubisoft account details from the Ubisoft username 
``` http
    Post /PlayerAccounts/GetAndUpdatePlayerAccounts
```
An endpoint that takes in one query parameter.   
It will return an object containing the Ubisoft names and IDs.  
Has two use cases:  
1) In register to get the new users ubisoft id.  
2) To get the data for the players that the user wants to compare to

| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `ubisoftUsernamesCommaSeperated` | `string` | **Required**. Contains the username/s. If more than one username the usernames needs to be seperated by comma|

Example request:
```shell
Api/PlayerAccounts/GetAndUpdatePlayerAccounts?ubisoftUsernamesCommaSeperated=player1,player2 \
  --request POST

```

Example response:   
```json
[
  {
    "ubisoftUsername": "player1",
    "ubisoftUserId": "00000000-bc24-414a-b9f1-81954236c34b"
  },
  {
    "ubisoftUsername": "player2",
    "ubisoftUserId": "00000000-e549-470b-894d-abe78a2e7f6e"
  }
]
```
### Get map times
``` http
    POST /MapTime/GetAllMapTimes
```
An endpoint for gathering the times on a selected map for the user, the players the user has chosen to compare to and the world recond.   
Requires two query parameters and a JSON body with an object contaning Ubisoft names and IDs.

Query parameters:
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `mapId` | `string` | **Required**. Contains the mapId to get times for everyone but the world record  |
| `mapUid` | `string` | **Required**. Contains the mapUid to get the map world record, since you can not the that with the mapId  |

JSON Body object:  
| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `ubisoftUserName` | `string` | **Required**. Contains an ubisoftUserName  |
| `ubisoftUserId` | `string` | **Required**. Contains an ubisoftUserId  |

Example request: 
```shell
curl 'Api/MapTime/GetAllMapTimes?mapId=&mapUid=' \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '[
  {
    "ubisoftUsername": "",
    "ubisoftUserId": ""
  }
]'

```

Example response: 
```json
[
  {
    "accountId": "longUbisoftAccountId",
    "name": "USER",
    "medal": 4,
    "medalText": "AUTHOR",
    "recordScore": {
      "time": 23304,
      "formatedTime": "00:23.304",
      "timeVsWr": 333
    }
  }
]
```



### Get campagin maps from the database 

```http
  GET /Map/GetMapsByYearAndSeason
```

Gets 25 campagin maps from the selected season and year.     
NB: 2025 fall is not a valid combo, since the fall campagin of 2025 has yet to be released. 
| Parameter | Type     | Description                       |
| :-------- | :------- | :-------------------------------- |
| `year`      | `integer` | **Required**. The valid years are: 2023,2024,2025|
| `season`      | `string` | **Required**. The valid seasons are: winter,spring,summer,fall  |

Example request:  
```shell
curl 'Api/Map/GetMapsByYearAndSeason?year=2024&season=winter'
```

Example response:
``` json
[
    {
        "mapId": "25b39d58-67bf-4024-8b3b-618234423bab"
        "mapUid":"Kn63nCh9bkaRHcZZsrtf_KcLMH2"
        "name": "Summer 2025 - 01"
    },
    {
        "mapId": "27a7a997-a812-49be-8e42-2e5414f9b529"
        "mapUid":"79yeZ2TL6FueaaP_2E9K6UWFIpb"
        "name": "Summer 2025 - 02"
    }
]
```

## Lessons Learned

1. Sending requests to external API's to acquire tokens and data   
   
   When i started out on this project, the first big problem i had was how to contact an external API to acquire the data i needed for this project.  
   It seemed very complicated at first, since i had no knowledge in how to build a request and how to handle the response data.   
   The first breaktrough came when i  was able to acquire my first Ubisoft ticket which i then could use to acquire my first Nadeo token.   
   Eventually i was able to automate acquiring tokens, which was a very fun challenge. 

2. Setting up API endpoints for data requests

When i started out on this project i had barely touched API's.   
But through this project i have learned how to setup API endpoints to get data from my database and from external API's.

3. Handling of JSON data
   
   This project taught me a lot about how to handle JSON data and how to send JSON data!  
   When i started i had some experience with handling JSON data, but not nearly enough!  
   I learned how to set up classes for deserilizing and through trial and error how to deserilize the data without any errors.   
   I also ended up learning how to use JSON to send data from requests.   

4. Acquired more database knowledge  
I had worked a bit with databases before this project, but i learnt a lot about good database structering.   
I also learnt how to to send database queries using Entityframework and how to use LINQ to filter the data!

5. Making better and less clutered code
This is something i have focused a lot on before, but it quickly becomes hard to manage when your project starts to grow.   
So i ended up spending a lot of time focusing on not repeating code and making the code more readable.
## Environment Variables

To run this project, you will need to add the following variables to your .NET user secrets file

`UbisoftEmail` You need an Ubisoft Email that is linked to the account you wish to use for Authentication

`UbisoftPassword` You also the password for your Ubisoft account
   
   You can read more about that here: https://webservices.openplanet.dev/auth/ubi


`ConnectionStrings:YourDatabaseOfChoice`: "valid connection string"   

`OAuth2Identifier`: "IdentiferKey"  

`OAuth2Secret`: "SecretKey"  

To learn more about how to aquire The two OAuth keys go to this link:https://webservices.openplanet.dev/oauth/auth


## Acknowledgements

 - [Openplanet](https://webservices.openplanet.dev/)
 - [Readme.so](https://readme.so)
