var envFile = require('./env');
envFile.Load();

var express = require('express');

var path = require('path');
var cookieParser = require('cookie-parser');
var logger = require('morgan');

/* PostgreSQL connection */
const { Client } = require('pg');
const { query } = require('express');
const postgresqlClient = new Client({
  host:     process.env.SERVER_URL,
  database: process.env.DATABASE_NAME,
  user:     process.env.DATABASE_USER,
  password: process.env.DATABASE_USER_PASSWORD,
  port:     process.env.DATABASE_PORT
});

postgresqlClient.connect();
console.log('Connected to the PostgreSQL database.')

/* Express set-up */
var app = express();

app.set('view engine', 'pug');

app.use(logger('dev'));
app.use(express.json());
app.use(express.urlencoded({ extended: false }));
app.use(cookieParser());

/* GET home page */
app.get('/', function(req, res, next) {
  res.render('index');
});

/* GET search results */
app.get('/search', function(req, res, next) {
  const searchQuery = req.query.search;
  let resultData = [];

  // Extremely simple search algorithm, look for the
  // search query in the website's description
  //
  // Each search is limited to 25 results
  // The result limit is not ideal, but it is good
  // enough for a proof of concept
  //
  // The input is parsed using a Regex that matches
  // all words and numbers. No input sanitization is
  // done, which poses a security threat.
  //
  // Be sure to validate all your user input in a
  // production environment!
  const splitWords = searchQuery.match(/\w+|\d+/);
  let queryParams = [];
  splitWords.forEach(word => {
    queryParams.push('%' + word + '%');
  });

  const sql = `SELECT * FROM ${process.env.TABLE_NAME} WHERE description LIKE ANY (array[$1]) LIMIT 25;`;

  postgresqlClient.query(sql, queryParams, (err, result) => {
    if (err) {
      console.log(err.stack);
      return;
    } else {
      result.rows.forEach(row => {
        resultData.push({
          url: row.url,
          title: row.title,
          description: row.description
        });
      });

      res.render('search_results', {
        query: req.query.search,
        results: resultData
      });
    }
  });
});

/* Set the static serving directory after the routes to ensure console.log()
   works correctly. Logging to the console will no longer after this functino */
app.use(express.static(path.join(__dirname, 'public')));

module.exports = app;
