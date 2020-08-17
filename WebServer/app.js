var express = require('express');

var path = require('path');
var cookieParser = require('cookie-parser');
var logger = require('morgan');

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
  res.render('search_results', { query: req.query.search });
});

/* Set the static serving directory after the routes to ensure console.log()
   works correctly. Logging to the console will no longer after this functino */
app.use(express.static(path.join(__dirname, 'public')));

module.exports = app;
