var express = require('express');

var path = require('path');
var cookieParser = require('cookie-parser');
var logger = require('morgan');

var app = express();

app.use(logger('dev'));
app.use(express.json());
app.use(express.urlencoded({ extended: false }));
app.use(cookieParser());

/* GET home page */
app.get('/', function(req, res, next) {
  console.log(__dirname);
  res.sendFile(path.join(__dirname, 'public', 'index.html'));
});

/* GET search results */
app.get('/search', function(req, res, next) {
  console.log("Search query: " + req.query.search);
  res.sendFile(path.join(__dirname, 'public', 'search_results.html'));
});

/* Set the static serving directory after the routes to ensure console.log()
   works correctly. Logging to the console will no longer after this functino */
app.use(express.static(path.join(__dirname, 'public')));

module.exports = app;
