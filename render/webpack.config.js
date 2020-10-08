'use strict'

const TerserJSPlugin = require('terser-webpack-plugin');
const path = require('path');
const webpack = require('webpack');
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const { CleanWebpackPlugin } = require('clean-webpack-plugin');
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin;
const HtmlWebpackPlugin = require('html-webpack-plugin');
const { VueLoaderPlugin } = require('vue-loader');
const OptimizeCSSAssetsPlugin = require('optimize-css-assets-webpack-plugin');

const isDevBuild = !(process.env.NODE_ENV && process.env.NODE_ENV === 'production')

const PORT = 8080;

module.exports = {
  mode: (isDevBuild ? 'development' : 'production'),
  devServer: {
    clientLogLevel: 'warning',
    hot: true,
    contentBase: path.join(__dirname, 'dist'),
    compress: true,
    port: PORT,
    open: true,
    overlay: { warnings: false, errors: true },
    publicPath: '/',
    quiet: true
  },
  entry: {
    main: './src/main.js',
  },
  resolve: {
    extensions: ['.js', '.vue'],
    alias: {
      'components': path.resolve(__dirname, './src/components'),
    }
  },
  module: {
    rules: [
      { test: /\.vue$/, use: 'vue-loader' },
      {
        test: /\.script\.js$/,
        use: 'script-loader'
      },
      {
        test: /\.css$/,
        use: [
          MiniCssExtractPlugin.loader,
          {
            loader: 'css-loader',
            options: { url: true }
          }
        ]
      },
      {
        test: /\.(png|jpg|jpeg|gif|svg)$/,
        use: {
          loader: 'url-loader',
          options: {
            limit: 25000, // if less than 10 kb, add base64 encoded image to css
            name: "assets/[hash].[ext]" // if more than 10 kb move to this folder in build using file-loader
          }
        }
      }
      // {
      //   test: /\.(woff(2)?|ttf|eot|svg)(\?v=\d+\.\d+\.\d+)?$/,
      //   use: [
      //     {
      //       loader: 'file-loader',
      //       options: {
      //         name: '[name].[ext]',
      //         outputPath: 'fonts/'
      //       }
      //     }
      //   ]
      // }
      // {
      //   test: /\.(woff|woff2|ttf|svg|eot|png|jpg|jpeg|gif|svg)$/,
      //   use: {
      //     loader: 'url-loader'
      //   },
      // }
    ]
  },
  output: {
    path: path.resolve(__dirname, './dist'),
    filename: "[name].bundle.js"
  },
  // optimization: {
  //   minimizer: [new TerserJSPlugin({}), new OptimizeCSSAssetsPlugin({})],
  //   splitChunks: {
  //     cacheGroups: {
  //       commons: {
  //         test: /[\\/]node_modules[\\/]/,
  //         name: "vendor",
  //         chunks: "all",
  //       },
  //     },
  //   },
  // },
  watch: true,
  devtool: 'cheap-inline-modules-source-map',
  watchOptions: {
    aggregateTimeout: 100
  },
  plugins: [
    new VueLoaderPlugin(),
    new HtmlWebpackPlugin({
      template: 'src/index.html',
      inject: false
    }),
    new CleanWebpackPlugin(),
    new MiniCssExtractPlugin({
      filename: "[name].bundle.css",
      chunkFilename: '[name].css'
    }),
    new webpack.HotModuleReplacementPlugin(),

    //new BundleAnalyzerPlugin()
  ]
}