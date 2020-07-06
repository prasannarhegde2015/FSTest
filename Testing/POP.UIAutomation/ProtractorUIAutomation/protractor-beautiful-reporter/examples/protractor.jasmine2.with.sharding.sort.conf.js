var HtmlReporter = require('protractor-beautiful-reporter');
var path = require('path');

// ----- Config example for Jasmine 2 -----

exports.config = {
    // ----- How to setup Selenium -----
    //
    // There are three ways to specify how to use Selenium. Specify one of the
    // following:
    //
    // 1. seleniumServerJar - to start Selenium Standalone locally.
    // 2. seleniumAddress - to connect to a Selenium server which is already
    //    running.
    // 3. sauceUser/sauceKey - to use remote Selenium servers via SauceLabs.

    // The location of the selenium standalone server .jar file.
    //seleniumServerJar: 'node_modules/protractor/selenium/selenium-server-standalone-2.37.0.jar',

    // Address of selenium server (before running protractor, run "webdriver-manager start" to start the Selenium server)
    seleniumAddress: 'http://localhost:4444/wd/hub',

    // The port to start the selenium server on, or null if the server should
    // find its own unused port.
    seleniumPort: null,

    // Chromedriver location is used to help the selenium standalone server
    // find chromedriver. This will be passed to the selenium jar as
    // the system property webdriver.chrome.driver. If null, selenium will
    // attempt to find chromedriver using PATH.
    //chromeDriver: 'node_modules/protractor/selenium/chromedriver',

    // Additional command line options to pass to selenium. For example,
    // if you need to change the browser timeout, use
    // seleniumArgs: ['-browserTimeout=60'],
    seleniumArgs: [],

    // If sauceUser and sauceKey are specified, seleniumServerJar will be ignored.
    // The tests will be run remotely using SauceLabs.
    sauceUser: null,
    sauceKey: null,

    // ----- What tests to run -----
    //
    // Spec patterns are relative to the location of this config.
    specs: [
        './specs/*.js'
    ],

    // ----- Capabilities to be passed to the webdriver instance ----
    //
    // For a full list of available capabilities, see
    // https://code.google.com/p/selenium/wiki/DesiredCapabilities
    // and
    // https://code.google.com/p/selenium/source/browse/javascript/webdriver/capabilities.js
    capabilities: {
        browserName: 'chrome',
        logName: 'Chrome - English',
        version: '',
        platform: 'ANY',
        shardTestFiles: true,
        maxInstances: 2,
    },

    // A base URL for your application under test. Calls to protractor.get()
    // with relative paths will be prepended with this.
    baseUrl: 'http://localhost:9999',

    // Set the framework
    framework: 'jasmine',

    // Selector for the element housing the angular app - this defaults to
    // body, but is necessary if ng-app is on a descendant of <body>
    rootElement: 'body',

    onPrepare: function () {
        // Add a screenshot reporter:
        jasmine.getEnv().addReporter(new HtmlReporter({
            preserveDirectory: true,
            takeScreenShotsOnlyForFailedSpecs: true,
            screenshotsSubfolder: 'images',
            jsonsSubfolder: 'jsons',
            baseDirectory: 'reports-tmp',
            pathBuilder: function pathBuilder(spec, descriptions, results, capabilities) {
                // Return '<30-12-2016>/<browser>/<specname>' as path for screenshots:
                // Example: '30-12-2016/firefox/list-should work'.
                var currentDate = new Date(),
                    day = currentDate.getDate(),
                    month = currentDate.getMonth() + 1,
                    year = currentDate.getFullYear();

                var validDescriptions = descriptions.map(function (description) {
                    return description.replace('/', '@');
                });

                return path.join(
                    day + "-" + month + "-" + year,
                    // capabilities.get('browserName'),
                    validDescriptions.join('-'));
            },
            sortFunction: function sortFunction(a, b) {
                if (a.cachedBase === undefined) {
                    var aTemp = a.description.split('|').reverse();
                    a.cachedBase = aTemp.slice(0).slice(0,-1);
                    a.cachedName = aTemp.slice(0).join('');
                };
                if (b.cachedBase === undefined) {
                    var bTemp = b.description.split('|').reverse();
                    b.cachedBase = bTemp.slice(0).slice(0,-1);
                    b.cachedName = bTemp.slice(0).join('');
                };

                var firstBase = a.cachedBase;
                var secondBase = b.cachedBase;

                for (var i = 0; i < firstBase.length || i < secondBase.length; i++) {

                    if (firstBase[i] === undefined) { return -1; }
                    if (secondBase[i] === undefined) { return 1; }
                    if (firstBase[i].localeCompare(secondBase[i]) === 0) { continue; }
                    return firstBase[i].localeCompare(secondBase[i]);
                }

                var firstTimestamp = a.timestamp;
                var secondTimestamp = b.timestamp;

                if(firstTimestamp < secondTimestamp) return -1;
                else return 1;
            }
        }).getJasmine2Reporter());
    },

    jasmineNodeOpts: {
        // If true, print colors to the terminal.
        showColors: true,
        // Default time to wait in ms before a test fails.
        defaultTimeoutInterval: 30000,
    }
};

