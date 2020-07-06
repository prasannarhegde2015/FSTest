var PageObject=require('../PageObjects.json');
var env=require('../envconfig.json');
var utility=require('../utility.js');
var data=require('../Datas.json');
var remote=require('../../node_modules/selenium-webdriver/remote');
var path = require('path');


describe('Well Search ||Global Areas', function() {
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
	
	it('Wild Card Well Search', function() {
		var createdlastwell;			
		var rrlwellname=String(utility.wellcreate("RRL")); var rrllength=rrlwellname.length;
		utility.waitforload();
		var espwellname=String(utility.wellcreate("ESP")); var esplength=espwellname.length;
		//console.log(rrlwellname); console.log(espwellname);
		var char1rrl=rrlwellname.charAt(0);	var char1esp=espwellname.charAt(0);
		var char2rrl=rrlwellname.charAt(1);	var char2esp=espwellname.charAt(1);		
		var char3rrl=rrlwellname.charAt(rrllength-1);	var char4esp=espwellname.charAt(esplength-1);
		var char4rrl=rrlwellname.charAt(rrllength-2);	var char5esp=espwellname.charAt(esplength-2);
		utility.waitclick(utility.findbyxpath(PageObject.selectwelldropdown),"Select well dropdown");
		utility.waitclick(utility.findbyxpath(PageObject.wellsearchinput),"Well Search Input");
		utility.findbyxpath(PageObject.wellsearchinput).sendKeys(char1rrl);
		browser.driver.sleep(500);//for a gap between keystrokes
		utility.findbyxpath(PageObject.wellsearchinput).sendKeys(char2rrl);	
		browser.driver.sleep(500);//for a gap between keystrokes
		utility.findbyxpath(PageObject.wellsearchinput).sendKeys("*");	
		browser.driver.sleep(4000);//for a gap between keystrokes
		browser.waitForAngular();
	//	browser.driver.sleep(4000);	
		element.all(by.xpath(PageObject.returnwelllist)).then(function(allwells){
			let wellsize;
			element.all(by.xpath(PageObject.returnwelllist)).count().then(function(size){ wellsize=size;console.log("Well Count is"+wellsize);});
			
			if(!wellsize==0){expect(true).toBe(false, "No wells returning as per wild card search, as expected it should return");}
			allwells.forEach(element => {
				element.getText().then(function(text){
					//the application at the moment returning 20 wells, after scrolling will return additional result which is similar to lazy loading
					if(text.charAt(0)==char1rrl && text.charAt(1)==char2rrl){
						console.log("Returning text after applying albhabets* = "+text);						
					//	console.log("Return result at "+a+" position is matching with search criteria");
						expect(true).toBe(true);
					}	
					else{
						//console.log("Return result at "+a+" position is not matching with search criteria");
						expect(true).toBe(false, "Result Well not matching with search criteria");
					}
				});
			});			
		});
		
		utility.waitclick(utility.findbyxpath(PageObject.wellsearchinput),"Well Search Input");
		utility.sendspecialkey(utility.findbyxpath(PageObject.wellsearchinput),"selectall");
		utility.sendspecialkey(utility.findbyxpath(PageObject.wellsearchinput),"delete");
		utility.findbyxpath(PageObject.wellsearchinput).sendKeys("*");
		browser.driver.sleep(500);	
		utility.findbyxpath(PageObject.wellsearchinput).sendKeys(char5esp);
		browser.driver.sleep(500);//for a gap between keystrokes
		utility.findbyxpath(PageObject.wellsearchinput).sendKeys(char4esp);			
		browser.driver.sleep(4000);//for a gap between keystrokes
		browser.waitForAngular();
	//	browser.driver.sleep(4000);	
		element.all(by.xpath(PageObject.returnwelllist)).then(function(allwells){
			let wellsize;
			element.all(by.xpath(PageObject.returnwelllist)).count().then(function(size){ wellsize=size;console.log("Well Count is"+wellsize);});
			if(wellsize==0){expect(true).toBe(false, "No wells returning as per wild card search, as expected it should return");}
			allwells.forEach(element => {
				element.getText().then(function(text1){
					//the application at the moment returning 20 wells, after scrolling will return additional result which is similar to lazy loading
					if(text1.charAt(esplength-1)==char4esp && text1.charAt(esplength-2)==char5esp){
						console.log("Returning text after applying *albhabets= "+text1);
					//	console.log("Return result at "+a+" position is matching with search criteria");
						expect(true).toBe(true);
					}	
					else{
						//console.log("Return result at "+a+" position is not matching with search criteria");
						expect(true).toBe(false, "Result Well not matching with search criteria");
					}
				});
			});			
		});
		
		utility.waitclick(utility.findbyxpath(PageObject.wellsearchinput),"Well Search Input");
		utility.sendspecialkey(utility.findbyxpath(PageObject.wellsearchinput),"selectall");
		utility.sendspecialkey(utility.findbyxpath(PageObject.wellsearchinput),"delete");
		utility.findbyxpath(PageObject.wellsearchinput).sendKeys(char1esp);
		browser.driver.sleep(500);//for a gap between keystrokes
		utility.findbyxpath(PageObject.wellsearchinput).sendKeys(char2esp);
		browser.driver.sleep(500);	
		utility.findbyxpath(PageObject.wellsearchinput).sendKeys("*");
		browser.driver.sleep(500);	
		utility.findbyxpath(PageObject.wellsearchinput).sendKeys(char5esp);
		browser.driver.sleep(500);//for a gap between keystrokes
		utility.findbyxpath(PageObject.wellsearchinput).sendKeys(char4esp);			
		browser.driver.sleep(4000);//for a gap between keystrokes
		browser.waitForAngular();
	//	browser.driver.sleep(4000);	
		element.all(by.xpath(PageObject.returnwelllist)).then(function(allwells){
			let wellsize;
			element.all(by.xpath(PageObject.returnwelllist)).count().then(function(size){ wellsize=size;console.log("Well Count is"+wellsize);});
			if(wellsize==0){expect(true).toBe(false, "No wells returning as per wild card search, as expected it should return");}
			allwells.forEach(element => {
				element.getText().then(function(text1){
					//the application at the moment returning 20 wells, after scrolling will return additional result which is similar to lazy loading
					if(text1.charAt(0)==char1esp && text1.charAt(1)==char2esp && text1.charAt(esplength-1)==char4esp && text1.charAt(esplength-2)==char5esp){
						console.log("Returning text after applying albhabets*albhabets= "+text1);
					//	console.log("Return result at "+a+" position is matching with search criteria");
						expect(true).toBe(true);
					}	
					else{
						//console.log("Return result at "+a+" position is not matching with search criteria");
						expect(true).toBe(false, "Result Well not matching with search criteria");
					}
				});
			});			
		});				
		browser.driver.navigate().refresh() 	
	});	
	
});
describe('WellConfiguration||Configuration', function() {
	beforeEach(function() {
		originalTimeout = jasmine.DEFAULT_TIMEOUT_INTERVAL;
		jasmine.DEFAULT_TIMEOUT_INTERVAL = 100000;
		browser.waitForAngularEnabled(false);
		browser.driver.manage().window().maximize();
		});
	afterEach(function() {
			
		
		});
	beforeAll(function() {
			
		});
	afterAll(function() {
		
		});
	
	it('RRL Creation', function() {	
			
		
	});
	it('ESP Creation', function() {	
		

	});
	
});
describe('Network Configuration||Configuration', function() {
	beforeEach(function() {
			originalTimeout = jasmine.DEFAULT_TIMEOUT_INTERVAL;
			jasmine.DEFAULT_TIMEOUT_INTERVAL = 100000;
		});
	afterEach(function() {
			
		});
	beforeAll(function() {
			
		});
	afterAll(function() {
		
		});
	
	it('Check RRL Network', function() {
		

	});
	
});

