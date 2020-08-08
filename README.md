# Welcome
Hi there, this repository is a small personal project of mine. One day I wondered how search engines such as Google, DuckDuckGo, and Bing! worked. This is why I set out on a quest to build my own simple search engine.

# Structure
The search engine is powered by a PostgreSQL database. A custom web crawler is used to index websites and save them to the database.

This database is then used by the search engine server to display results.

- `/WebScraper` contains the web crawler, folder needs to be renamed as it is not a scraper.
- `/WebServer` contains the NodeJS / ExpressJS webserver used to power the search engine.

# Contributing
I always welcome pull-requests and issues. So if you have any suggestions or improvements, feel free to let me know by submitting a ticket on GitHub.

Cheers!
