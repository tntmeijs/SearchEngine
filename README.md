# Welcome
Hi there, this repository is a small personal project of mine. One day I wondered how search engines such as Google, DuckDuckGo, and Bing! worked. This is why I set out on a quest to build my own simple search engine.

The initial goal of this project was to build a working search engine. I did not feel the need to polish it and really turn it into a usable search engine. This was just a project to help me learn NodeJS, .NET, and PostgreSQL.

Demonstration: [https://www.youtube.com/watch?v=b5r_njqtAkc](https://www.youtube.com/watch?v=b5r_njqtAkc)

# Known issues / future improvements
- Web crawler sometimes fails to respect more complex robots.txt files.
- Use multithreading to improve overall crawl speed. Indexing pages can be easily done in parallel.
- Search engine web pages could use style improvements. It's pretty dull right now.
- The PostgreSQL database is a bit of a mess, can be split up into multiple tables to make it easier to manage.
- Back-up system to avoid losing indexed data.
- Implement [http://ilpubs.stanford.edu:8090/422/1/1999-66.pdf](Page Rank) to improve search results.
- Split the crawler and search engine backend database into two to avoid writing to a production database.

# Structure
The search engine is powered by a PostgreSQL database. A custom web crawler is used to index websites and save them to the database.

This database is then used by the search engine server to display results.

- `/WebScraper` contains the web crawler, folder needs to be renamed as it is not a scraper.
- `/WebServer` contains the NodeJS / ExpressJS webserver used to power the search engine.

# Contributing
I always welcome pull-requests and issues. So if you have any suggestions or improvements, feel free to let me know by submitting a ticket on GitHub.

Cheers!
