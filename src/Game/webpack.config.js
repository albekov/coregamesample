/// <binding ProjectOpened='Watch - Development' /> 

var webpack = require('webpack');
var WebpackNotifierPlugin = require('webpack-notifier');

module.exports = {
    entry: {
        'app': './src/app.ts'
    },
    output: {
        path: './wwwroot/',
        filename: './dist/[name].bundle.min.js'
    },

    devtool: 'source-map',

    resolve: {
        extensions: ['', '.webpack.js', '.ts', '.js']
    },

    plugins: [
        new webpack.optimize.UglifyJsPlugin({
            mangle:false
        }),
        new WebpackNotifierPlugin()
    ],

    module: {
        loaders: [
            { test: /\.ts$/, loader: 'ts-loader' }
        ],

        preLoaders: [
            { test: /\.js$/, loader: 'source-map-loader' }
        ]
    }
}