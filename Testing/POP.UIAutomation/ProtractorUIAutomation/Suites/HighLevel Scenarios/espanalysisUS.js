var PageObject=require('../PageObjects.json');
var env=require('../envconfig.json');
var utility=require('../utility.js');
var data=require('../Datas.json');
var remote=require('../../node_modules/selenium-webdriver/remote');
var path = require('path');

describe('WellAnalysis||Optimization', function() {
	beforeEach(function() {				
		browser.get('http://localhost:2678/#/dashboards/pedashboard');		
		expect(browser.getTitle()).toEqual('Weatherford ForeSite');
		
	});
	afterEach(function() {
		
	});
	beforeAll(function() {
		originalTimeout = jasmine.DEFAULT_TIMEOUT_INTERVAL;
		jasmine.DEFAULT_TIMEOUT_INTERVAL = 100000;
		browser.waitForAngularEnabled(false);
		browser.driver.manage().window().maximize();
	
	});
	afterAll(function() {
		
	});
	it('ESP Analysis US unit', function() {
		
		var name=utility.wellcreate("ESP");
		console.log("Created ESP name is: "+name);
		utility.welltest("ESP", "us");
		utility.getspantext(utility.findbyxpath(PageObject.OptimizationPage.qualitycodecol), "Quality code column element could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();
			console.log("Text Found:- "+text);
			expect(text).toEqual(data.espwelltestdata.qualitycodetext, "Quality code text not matching");
		});
		utility.getspantext(utility.findbyxpath(PageObject.OptimizationPage.tunningmethodcol), "Tunnign Method  code column element could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();
			console.log("Text Found:- "+text);
			expect(text).toEqual(data.espwelltestdata.tunningmethod, "Tunnign Method code text not matching");
		});

		utility.getspantext(utility.findbyxpath(PageObject.OptimizationPage.statuscol), "Status code column element could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();
			console.log("Text Found:- "+text);
			expect(text).toEqual("success", "Status code code text not matching");
		});
		
		utility.getspantext(utility.findbyxpath(PageObject.OptimizationPage.thpcol), "THP Col code column element could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();
			console.log("Text Found:- "+text);
			expect(text).toEqual(data.espwelltestdata.thp, "THP code code text not matching");
		});
		utility.getspantext(utility.findbyxpath(PageObject.OptimizationPage.thtcol), "THT Col code column element could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();
			console.log("Text Found:- "+text);
			expect(text).toEqual(data.espwelltestdata.tht, "THT code code text not matching");
		});
		utility.getspantext(utility.findbyxpath(PageObject.OptimizationPage.chpcol), "CHP Col code column element could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();
			console.log("Text Found:- "+text);
			expect(text).toEqual(data.espwelltestdata.chp, "CHP code code text not matching");
		});
		utility.getspantext(utility.findbyxpath(PageObject.OptimizationPage.freqcol), "FREQ Col code column element could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();
			console.log("Text Found:- "+text);
			expect(text).toEqual(data.espwelltestdata.freq, "FREQ code code text not matching");
		});
		utility.getspantext(utility.findbyxpath(PageObject.OptimizationPage.flpcol), "FREQ Col code column element could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();
			console.log("Text Found:- "+text);
			expect(text).toEqual(data.espwelltestdata.flp, "FREQ code code text not matching");
		});
		utility.getspantext(utility.findbyxpath(PageObject.OptimizationPage.spcol), "SP code column element could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();
			console.log("Text Found:- "+text);
			expect(text).toEqual(data.espwelltestdata.sp, "SP code code text not matching");
		});
		utility.getspantext(utility.findbyxpath(PageObject.OptimizationPage.chokesizecol), "CHOKESIZE code column element could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();
			console.log("Text Found:- "+text);
			expect(text).toEqual(data.espwelltestdata.chokesize, "CHOKESIZE code code text not matching");
		});
		utility.getspantext(utility.findbyxpath(PageObject.OptimizationPage.oilratecol), "oilrate code column element could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();
			console.log("Text Found:- "+text);
			expect(text).toEqual(data.espwelltestdata.oilrate, "Oilrate code text not matching");
		});
		utility.getspantext(utility.findbyxpath(PageObject.OptimizationPage.gasratecol), "gas rate code column element could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();
			console.log("Text Found:- "+text);
			expect(text).toEqual(data.espwelltestdata.gasrate, "gas rate code code text not matching");
		});
		utility.getspantext(utility.findbyxpath(PageObject.OptimizationPage.waterratecol), "waterrate code column element could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();
			console.log("Text Found:- "+text);
			expect(text).toEqual(data.espwelltestdata.waterrate, "water rate code code text not matching");
		});
	
		utility.waitclick(utility.findbyxpath(PageObject.OptimizationPage.wellanalysis));
		utility.waitforload();
		utility.waitclick(utility.findbyxpath(PageObject.OptimizationPage.dailyaveragedataswitch));
		utility.waitforload();
		utility.getinputtext(utility.findbyxpath(PageObject.OptimizationPage.qualitycodeinput), "Quality code in wellanalysis screen could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();
			console.log("Well Analysis screen Text Found:- "+text);
			expect(text).toEqual(data.espwelltestdata.qualitycodetext, "Quality code in wellanalysis screen text not matching");
		});
		utility.getinputtext(utility.findbyxpath(PageObject.OptimizationPage.wellhpresswellanalysis), "WHP code in wellanalysis screen could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();
			console.log("Well Analysis screen Text Found:- "+text);
			expect(text).toEqual(data.espwelltestdata.thp, "WHP code in wellanalysis screen text not matching");
		});
		utility.getinputtext(utility.findbyxpath(PageObject.OptimizationPage.wellhtemwellanalysis), "WHT code in wellanalysis screen could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();
			console.log("Well Analysis screen Text Found:- "+text);
			expect(text).toEqual(data.espwelltestdata.tht, "WHT code in wellanalysis screen text not matching");
		});
		utility.getinputtext(utility.findbyxpath(PageObject.OptimizationPage.freqwellanalysis), "Freq code in wellanalysis screen could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();
			console.log("Well Analysis screen Text Found:- "+text);
			expect(text).toEqual(data.espwelltestdata.freq, "Freq code in wellanalysis screen text not matching");
		});
		
		utility.getinputtext(utility.findbyxpath(PageObject.OptimizationPage.waterratewellanalysis), "WaterRate code in wellanalysis screen could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();
			text= text.replace(",", "");
			console.log("Well Analysis screen Text Found:- "+text);
			expect(text).toEqual(data.espwelltestdata.waterrate, "WaterRate code in wellanalysis screen text not matching");
		});
		utility.getinputtext(utility.findbyxpath(PageObject.OptimizationPage.gasratewellanalysis), "GasRate code in wellanalysis screen could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();
			text= text.replace(",", "");
			console.log("Well Analysis screen Text Found:- "+text);
			expect(text).toEqual(data.espwelltestdata.gasrate, "GasRate code in wellanalysis screen text not matching");
		});
		utility.waitclick(utility.findbyxpath(PageObject.OptimizationPage.gradientcurvestab));

		utility.getspantext(utility.findbyxpath(PageObject.OptimizationPage.labelfortemp), "Temp label in wellanalysis screen could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();			
			console.log("Well Analysis screen Text Found:- "+text);
			expect(text).toEqual("temperature (°f)", "Temp Label in wellanalysis screen text not matching");
		});
		utility.getspantext(utility.findbyxpath(PageObject.OptimizationPage.labelfordep), "Depth label in wellanalysis screen could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();			
			console.log("Well Analysis screen Text Found:- "+text);
			expect(text).toEqual("depth (ft)", "Depth Label in wellanalysis screen text not matching");
		});
		utility.getspantext(utility.findbyxpath(PageObject.OptimizationPage.labelfortp), "Tubing Pressure in wellanalysis screen could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();			
			console.log("Well Analysis screen Text Found:- "+text);
			expect(text).toEqual("tubing pressure", "Tubing Pressure in wellanalysis screen text not matching");
		});
		utility.getspantext(utility.findbyxpath(PageObject.OptimizationPage.labelfortt), "Tubing Temperature in wellanalysis screen could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();			
			console.log("Well Analysis screen Text Found:- "+text);
			expect(text).toEqual("tubing temperature", "Tubing Temperature code in wellanalysis screen text not matching");
		});
		utility.getspantext(utility.findbyxpath(PageObject.OptimizationPage.labelforpress), "Pressure in wellanalysis screen could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();			
			console.log("Well Analysis screen Text Found:- "+text);
			expect(text).toEqual("pressure (psia)", "Pressure in wellanalysis screen text not matching");
		});
		utility.waitclick(utility.findbyxpath(PageObject.OptimizationPage.performancecurvestab));

		utility.getspantext(utility.findbyxpath(PageObject.OptimizationPage.labelforopfreq), "Operating freq in wellanalysis screen could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();			
			console.log("Well Analysis screen Text Found:- "+text);
			expect(text).toEqual("operating frequency: 60", "Operating freq Label in wellanalysis screen text not matching");
		});
		utility.getspantext(utility.findbyxpath(PageObject.OptimizationPage.labelforwtp), "Well test point in wellanalysis screen could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();			
			console.log("Well Analysis screen Text Found:- "+text);
			expect(text).toEqual("well test point", "Well test point Label in wellanalysis screen text not matching");
		});
		utility.waitclick(utility.findbyxpath(PageObject.OptimizationPage.inflowoutflowtab));
		utility.getspantext(utility.findbyxpath(PageObject.OptimizationPage.labelforpip), "PIP label in wellanalysis screen could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();			
			console.log("Well Analysis screen Text Found:- "+text);
			expect(text).toEqual("pip (psia)", "PIP Label in wellanalysis screen text not matching");
		});
		utility.getspantext(utility.findbyxpath(PageObject.OptimizationPage.labelforinflow), "Inflow label in wellanalysis screen could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();			
			console.log("Well Analysis screen Text Found:- "+text);
			expect(text).toEqual("inflow", "inflow Label in wellanalysis screen text not matching");
		});
		utility.getspantext(utility.findbyxpath(PageObject.OptimizationPage.labelforoutflow), "outflow label in wellanalysis screen could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();			
			console.log("Well Analysis screen Text Found:- "+text);
			expect(text).toEqual("outflow", "outflow Label in wellanalysis screen text not matching");
		});
		utility.getspantext(utility.findbyxpath(PageObject.OptimizationPage.labelforbubblepoint), "Bubble point label in wellanalysis screen could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();			
			console.log("Well Analysis screen Text Found:- "+text);
			expect(text).toEqual("bubble point", "Bubble point Label in wellanalysis screen text not matching");
		});
		utility.getspantext(utility.findbyxpath(PageObject.OptimizationPage.labelforqtech), "QTech label in wellanalysis screen could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();			
			console.log("Well Analysis screen Text Found:- "+text);
			expect(text).toEqual("q tech min-max", "QTech Label in wellanalysis screen text not matching");
		});
		utility.getspantext(utility.findbyxpath(PageObject.OptimizationPage.labelforoperatingpoint), "OP label in wellanalysis screen could not be found").then(function(text){
			text=text.toString().toLowerCase().trim();			
			console.log("Well Analysis screen Text Found:- "+text);
			expect(text).toEqual("operating point", "OP Label in wellanalysis screen text not matching");
		});
	});
	
	

});