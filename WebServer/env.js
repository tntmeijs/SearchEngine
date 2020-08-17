const dotenv = require('dotenv');

exports.Load = function() {
    const result = dotenv.config();
    if (result.error) {
        throw result.error;
    }

    console.log('Variables read from .env file:')
    console.log(result.parsed);
}
