# MediaStack

Mediastack is a media management tool that turns a directory of images/videos into a RESTful API. 

## Importing Media

### Directory Scanning:

Mediastack scans a specified directory and imports image and video files. It infers metadata from where the file is stored on disk.
The directory structure is as follows:

\<category>/\<artist>/\<album>/\<media>

The Category, Artist, and Album are directories, all of which are optional. Media is a file.

For example, if MediaStack found:

Drawings/Wilson/Cat.png

The metadata would look like:

```javascript
{
  "category" : "Drawings",
  "artist"   : "Wilson",
  "album"    : null,
  "path"     : "Drawings\Wilson\Cat.png",
  ...
}
```

### Filesystem Monitoring:

While MediaStack is running it will monitor the media directory and will update it's database when you add, delete or modify files.

## API Responses

All responses will be in the structure:

```json
{
  "message": "",
  "data": {}
}
```

`message` will always be a string and will report what went wrong if something goes wrong. `data` will always be an object or `null`.

## API Resources

All resources must be prefixed with the /api endpoint.

| RESOURCE | POST            | GET       | PUT         | DELETE |
| ----------- | --------------- | --------- | ----------- | ------ |
| `/categories` | [Creates a Category](#non-media_creation) | Returns a list of Categories | N/A | Deletes all Categories, and all Albums and Media within any Category |
| `/categories/<id>`  | N/A | Returns a list of Media and Albums in the Category | Edits all Albums and Media in Category | Deletes all Albums and Media in Category |
| `/artists` | [Creates an Artist](#non-media_creation) | Returns a list of Artists | N/A | Deletes all Artists, and all Albums and Media from any Artist |
| `/artists/<id>`  | N/A | Returns a list of Media and Albums from Artist | Edits all Albums and Media from Artist | Deletes all Albums and Media from Artist |
| `/albums` | [Creates an Album](#non-media_creation) | [Returns a list of Albums](#search_albums) | N/A | Deletes all Albums and all Media within any Album |
| `/albums/<id>`  | N/A | Returns Album info and all Media with the Album | [Edits Album and all Media info](#edit_album) | Deletes Album and all Media |
| `/media` | [Creates Media](#upload) | [Returns a list of Media](#search_media) | N/A | Deletes all Media |
| `/media/<id>`  | N/A | Returns the Media's info | [Edit's Media info](#edit_media) | Deletes Media info, file, and thumbnail |
| `/media/<id>/file`  | N/A | Returns the Media's file | N/A | N/A |
| `/media/<id>/thumbnail`  | N/A | Returns the Media's thumbnail | N/A | N/A |
| `/tags` | [Creates a Tag](#non-media_creation) | Returns a list of Tags | N/A | Deletes all Tags |
| `/tags/<id>` | N/A | Returns all Media and Albums that contain the Tag. | Edits all Albums and Media that have Tag | Deletes Tag |

### Creating Categories: [](#category_creation)

Send a POST request to the `/categories` resource with a json body containing the <u>unique</u> `name` of the category.

Example Body:
```json
{
  "name": "Drawings"
}
```

Example Response: (200)
```json
{
  "message": "",
  "data": {
    "name": "Drawings",
    "id": 1
  }
}
```

### Creating Artists: [](#artist_creation)

Send a POST request to the `/artists` resource with a json body containing the ID of a existing `category`, and the <u>unique</u> `name` of the artist.

Example Body:
```json
  {
    "name": "Wilson", 
    "category": 1
  }
```

Example Response: (200)
```json
{
  "message": "",
  "data": {
    "name": "Wilson",
    "id": 1,
    "media": [],
    "albums": [] 
  }
}
```

### Creating Albums: [](#album_creation)

Send a POST request to the `/albums` resource with a json body containing the ID of the `category`, the ID of the album's `artist`, and the `name` of the Album that is <u>unique to the artist</u>.

Example Body:
```json
  {
    "category": 1,
    "artist_id": 1,
    "name": "MS Paint Drawings", 
  }
```

Example Response: (200)
```json
{
  "message": "",
  "data": {
    "name": "MS Paint Drawings",
    "id": 1,
    "artist_id": 1,
    "category_id": 1,
    "media": [],
    "tags": []
  }
}
```

### Creating Tags: [](#tag_creation)

Send a POST request to the `/tags` resource with a json body containing the <u>unique</u> `name` of the tag.

Example Body:
```json
{
  "name": "Cat"
}
```

Example Response: (200)
```json
{
  "message": "",
  "data": {
    "name": "Cat",
    "id": 1
  }
}
```

### Searching Media: [](#search_media)

| Query Parameter | Description |
| ------ | ----------- |
| tag_ids | Returns Media that contain all Tags specified |
| blacklist_tag_ids | Returns Media that contain don't contain any of the Tags specified |
| score | Returns Media that have the exact score specified |
| gt_score | Returns Media that have a score greater than the one specified |
| lt_score | Returns Media that have a score less than the one specified |
| artist_id | Returns Media from the specified Artist |
| album_id | Returns Media from the specified Album |
| category_id | Returns Media in the specified Category |
| blacklist_artist_ids | Returns all Media excluding those from the specified Artists |
| blacklist_album_ids | Returns all Media excluding those from the specified Albums |
| blacklist_category_ids | Returns all Media excluding those from the specified Categories |

Example, to GET all Media that contain the Tag with the `ID` of 1 excluding the Category with the `ID` of 1:

/api/media?tags=1&blacklist_categories=1

### Searching Albums: [](#search_album)

| Query Parameter | Description |
| ------ | ----------- |
| tag_ids | Returns Albums that contain all Tags specified |
| blacklist_tag_ids | Returns Albums that don't contain any of the Tags specified |
| score | Returns Albums that have the exact score specified |
| gt_score | Returns Albums that have a score greater than the one specified |
| lt_score | Returns Albums that have a score less than the one specified |
| artist_id | Returns Albums from the specified Artist |
| category_id | Returns Albums in the specified Category |
| blacklist_artist_ids | Returns all Albums excluding those from the specified Artists |
| blacklist_category_ids | Returns all Albums excluding those from the specified Categories |

Example, to GET all Albums that have a score greater than 3:

/api/albums?gt_score=3

### Editting Media Information: [](#edit_media)

You can change a Media's Category, Artist, Album, Score, Source, and Tags.

To edit Media metadata send a PUT request to `/api/media/<id>` with a json body of the Media's metadata. If a field is not specified, it won't be changed.

Example Body to change the Media's category to the category with the `ID` of 2:
```json
  {
    "category_id": 2
  }
```

Example Response: (201)
```json
{
  "message": "",
  "data": {
    "id": 1,
    "category_id": 2,
    "artist_id": 1,
    "album_id": null,
    "score": 0,
    "source": "",
    "tag_ids": [1, 2]
  }
}
```

### Editting Album Information: [](#edit_album)

When you change an Album's metadata all changes are apply to all Media within the Album.

You can change a Albums's Category, Artist, Score, Source, and Tags.

To edit Album metadata send a PUT request to `/api/albums/<id>` with a json body of the Albums's metadata. If a field is not specified, it won't be changed.

Example Body to change the Albums's score to 3:
```json
  {
    "score": 3
  }
```

Example Response: (201)
```json
{
  "message": "",
  "data": {
    "id": 1,
    "category_id": 2,
    "artist_id": 1,
    "score": 3,
    "tag_ids": [1, 2],
    "media": [
      {
        "id": 1,
        "category_id": 2,
        "artist_id": 1,
        "album_id": 1,
        "score": 3,
        "source": "",
        "tag_ids": [1]
      },
      {
        "id": 2,
        "category_id": 2,
        "artist_id": 1,
        "album_id": 1,
        "score": 3,
        "source": "",
        "tag_ids": [2]
      }
    ]
  }
}
```

### Upload:

Still in development...

### Authorization

Still in development...

### Status Codes

Mediastack returns the following status codes:

| Status Code | Description |
| :--- | :--- |
| 200 | `OK` |
| 201 | `CREATED` |
| 400 | `BAD REQUEST` |
| 401 | `UNAUTHORIZED` |
| 403 | `FORBIDDEN` |
| 404 | `NOT FOUND` |
| 500 | `INTERNAL SERVER ERROR` |
| 503 | `SERVICE_UNAVAILABLE` |
