# AnimeQuiz

---

AnimeQuiz is a web app built by ASP .NET Core 8 MVC with Entity Framework Core as ORM, Bootstrap for styling, and JQuery for scripting.
It allows admin to manage content of an Anime Database, and then generate Anime Quiz from data in the Database to test users' knowledge of Anime.

[Presentation Video](https://humberital-my.sharepoint.com/:v:/g/personal/n01685197_humber_ca/EUlyQhHkdz1IsJ57sepL4B0B9DLqDUiKNnrqxg346CCnuQ?nav=eyJyZWZlcnJhbEluZm8iOnsicmVmZXJyYWxBcHAiOiJPbmVEcml2ZUZvckJ1c2luZXNzIiwicmVmZXJyYWxBcHBQbGF0Zm9ybSI6IldlYiIsInJlZmVycmFsTW9kZSI6InZpZXciLCJyZWZlcnJhbFZpZXciOiJNeUZpbGVzTGlua0NvcHkifX0&e=HpS0IV)

---

## Features

- CRUD for Anime Database
    - Create Anime, attach Images, Musics, and Characters to Anime
    - Create Character, attach Versions for the Character
    - Create Character Version, attach Images and Voice Actors to the Version
    - Create Staff, of which can be added to Music as Singer or Character as Voice Actor
    - Read Animes, single Anime, single Music, and Music Singer
    - Read Characters, single Character
    - Read Character Versions inside Character, single Character Version, and Voice Actor
    - Read Staffs, single Staff
    - Update Anime Name, Character Name, Version Name, and Staff Name
    - Delete Anime, Character, Character Version, Image, Music, Staff
    - Delete link of Anime to Character, Anime to Image, Anime to Music, and Music to Singer
    - Delete link of Character to Version, Version to Image, Version to Voice Actor
    
- Anime Quiz
    - Generate questions for quiz
    - Able to randomly generate questions of different types (e.g. Image or Music question)
    - The answer types differ from question to question too, let say for Music question it can ask about the Name or the Singer
    - The order of questions and choices are randomly shuffled
    - There is a 50/50 tool of limited chances which can help you to eliminate 2 incorrect answers
    
- Dropzone for file uploading
    - Multiple files can be uploaded by drag & drop or click
    - There is a preview for each Image you uploaded
    - There is a audio player for each Music you uploaded
    - File can be removed from the dropzone
    - There is a clear all button for removing all inputs
 
## Tech Stack

- ASP .NET Core 8 MVC app
- Entity Framework Core
- Bootstrap for styling
- JQuery for scripting

## Project Structures

- API Controllers: For handling API request
- MVC Controllers: For handling Page request
- Interfaces: For abstracting Services' methods
- Services: For underline backend logics
- Models: For typing
- Data: For data schema & migration
- Views: For rendering content

## API

### Anime
    - GET: /api/Anime ; Listing Animes
    - POST: /api/Anime ; Create an Anime
    - GET: /api/Anime/{id} ; Retrieve an Anime
    - PUT: /api/Anime/{id} ; Update an Anime
    - DELETE: /api/Anime/{id} ; Delete an Anime
    - POST: /api/Anime/{id}/CharacterVersion ; Link Characters to Anime
    - DELETE: /api/Anime/{id}/CharacterVersion ; Unlink Characters from Anime
    - POST: /api/Anime/{id}/Image ; Upload Images to Anime
    - DELETE: /api/Anime/{id}/Image ; Delete Images from Anime
    - POST: /api/Anime/{id}/Music ; Upload Musics to Anime
    - DELETE: /api/Anime/{id}/Music ; Delete Musics from Anime

### AnimeQuiz
    - GET: /api/AnimeQuiz?numOfQuestions=8 ; Generate an AnimeQuiz

### Character
    - GET: /api/Character ; Listing Characters
    - POST: /api/Character ; Create a Character
    - GET: /api/Character/{id} ; Retrieve a Character
    - PUT: /api/Character/{id} ; Update a Character
    - DELETE: /api/Character/{id} ; Delete a Character
    - POST: /api/Character/{id}/Version ; Add a Version to Character
    - DELETE: /api/Character/{id}/Version ; Delete Versions from Character

### CharacterVersion
    - GET: /api/CharacterVersion/{id} ; Retrieve a CharacterVersion
    - PUT: /api/CharacterVersion/{id} ; Update a CharacterVersion
    - POST: /api/CharacterVersion/{id}/Image ; Upload Images to CharacterVersion
    - DELETE: /api/CharacterVersion/{id}/Image ; Delete Images from CharacterVersion
    - POST: /api/CharacterVersion/{id}/VoiceActor ; Link VoiceActors to CharacterVersion
    - DELETE: /api/CharacterVersion/{id}/VoiceActor ; Unlink VoiceActors from CharacterVersion
 
### Music
    - POST: /api/Music/{id}/Singer ; Link Singers to Music
    - DELETE: /api/Music/{id}/Singer ; Unlink Singers from Music

### Staff
    - GET: /api/Staff ; Listing Staffs
    - POST: /api/Staff ; Create a Staff
    - GET: /api/Staff/{id} ; Retrieve a Staff
    - PUT: /api/Staff/{id} ; Update a Staff
    - DELETE: /api/Staff/{id} ; Delete a Staff
