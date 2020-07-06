// conf.js
var HtmlReporter = require('../ProtractorUIAutomation/protractor-beautiful-reporter');
//var jasmineReporters = require('../ProtractorUIAutomation/jasmine-reporters');
var reporter = new HtmlReporter({
   baseDirectory: 'Report', 
   gatherBrowserLogs: false,    
   docTitle: 'Foresite POP',
   takeScreenShotsOnlyForFailedSpecs: true
});

exports.config = {
  framework: 'jasmine',  
  seleniumAddress: 'http://localhost:4444/wd/hub',
  
  
  suites: {
   functional: 'Suites/Functional/*.js',
   regression: 'Suites/Regression/*.js',
   smoke: 'Suites/Smoke/*.js',
   highlevel: 'Suites/HighLevel Scenarios/*.js',
 },
  
  onPrepare: function() {
   
     // Add a screenshot reporter and store screenshots
      jasmine.getEnv().addReporter(new HtmlReporter({
        baseDirectory: 'Report'
      }).getJasmine2Reporter());

      //////////////////////////////////////////////////

     
    //jasmine.getEnv().addReporter(new jasmineReporters.TeamCityReporter({
      //  consolidateAll: true,
        //savePath: 'testresults',
        //filePrefix: 'xmloutput'
    //}));
   }
   
}