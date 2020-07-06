var PageObject=require('./PageObjects.json');
var env=require('./envconfig.json');
var datas=require('./Datas.json');
var remote=require('../node_modules/selenium-webdriver/remote');
var path = require('path');
var utility=function(){
this.waitclick=function(el, elname){	
	var until = protractor.ExpectedConditions;
	browser.wait(until.elementToBeClickable(el), 9000, elname+' taking too long to appear in the DOM');	
	el.click();
	};

this.toastverify=function(comptext, reasontext){
	var until = protractor.ExpectedConditions;
	var toast=element(by.xpath(PageObject.toastmessage));
	browser.wait(until.presenceOf(toast), 20000, 'Toast Message taking too long to appear in the DOM');	
	var text=element(by.xpath(PageObject.toasttext)).getText();
	expect(text).toContain(comptext, reasontext);		
	};
this.getinputtext=function(el, errortext){	
	var until = protractor.ExpectedConditions;
	browser.wait(until.elementToBeClickable(el), 9000, errortext);	
	var text=el.getAttribute('value');
	return text;
	};
this.getspantext=function(el, errortext){	
	var until = protractor.ExpectedConditions;
	browser.wait(until.elementToBeClickable(el), 5000, errortext);	
	var text=el.getText();
	return text;
	};
this.waitforload=function(){
	var until = protractor.ExpectedConditions;
	var loader=element(by.xpath("//div[@class='block-ui-wrapper block-ui-main active']"));
	browser.wait(until.invisibilityOf(loader), 50000, 'Element taking too long to appear in the DOM');
	};
this.sendtext=function(el, text){	
	var until = protractor.ExpectedConditions;
	browser.wait(until.elementToBeClickable(el), 5000, 'Element taking too long to appear in the DOM');	
	el.sendKeys(text);
	};
this.findbyxpath=function(data){	
	var el=element(by.xpath(data));
	return el;
	};
this.findbyid=function(data){	
	var el=element(by.id(data));
	return el;
	};
this.sendspecialkey=function(el,data){
	switch(data){
		
		case 'selectall':
		el.sendKeys(protractor.Key.chord(protractor.Key.CONTROL, "a")); 
		break;
		case 'delete':
		el.sendKeys(protractor.Key.chord(protractor.Key.DELETE));
		break;
		case 'tab':
		el.sendKeys(protractor.Key.chord(protractor.Key.TAB));
		break;
		default:
		expect("Keys to be valid").toEqual("Key values could not be found under sendspecialkey method");		
	}	
};	
this.wellcreate=function(welltype){
	var name;
	switch(welltype.toLowerCase()){
		case 'esp':
		this.waitforload();
		var wconf=this.findbyxpath(PageObject.WellConfigPage.wellconfigurationTab);
		var until = protractor.ExpectedConditions;
		browser.wait(until.elementToBeClickable(wconf), 8000)
		.then((found) => {
			wconf.click();
		})
		.catch((waitError) => {	 
			this.waitclick(this.findbyxpath(PageObject.WellConfigPage.configurationTab), "ConfigurationTab");
			this.waitforload();
			this.waitclick(this.findbyxpath(PageObject.WellConfigPage.wellconfigurationTab), "WellConfigTab");
		});		
		this.waitforload();
		var elem=this.findbyxpath(PageObject.WellConfigPage.createnew);
		var until = protractor.ExpectedConditions;
		browser.wait(until.elementToBeClickable(elem), 2000)
		.then((found) => {
			elem.click();
		})
		.catch((waitError) => {	 
		  
		});
		this.waitclick(this.findbyxpath(PageObject.WellConfigPage.general), "General Tab");		
		name="ESP"+this.getRandomString(4);
		this.findbyid(PageObject.WellConfigPage.wellnameinput).sendKeys(name);
		this.waitclick(this.findbyid(PageObject.WellConfigPage.welltypedropdwn),"WellType Dropdown");
		this.waitclick(this.findbyxpath(PageObject.WellConfigPage.welltypeESP), "WlTyp Name List");
		this.waitclick(this.findbyid(PageObject.WellConfigPage.scadatypedrpdwn),"SCada Dropdown"); 
		this.waitclick(this.findbyxpath(PageObject.WellConfigPage.scadatypename), "Scada Type Name"); 
		this.waitclick(this.findbyid(PageObject.WellConfigPage.cygnetdomaindrpdwn), "Cygnet Dropdown"); 
		this.waitclick(this.findbyxpath(env.cygnetdomainname),"Domain Name"); 
		this.waitforload();
		this.waitclick(this.findbyxpath(PageObject.WellConfigPage.cygnetservicedrpdwn),"Service Dropdown"); 
		this.waitclick(this.findbyxpath(env.cygnetservicename),"Service Name");
		this.waitforload();
		this.waitclick(this.findbyxpath(PageObject.WellConfigPage.facilitybutton),"Facility button");
		this.waitforload();
		this.waitclick(this.findbyxpath(PageObject.WellConfigPage.firstrowfacility),"First Facility in the list");
		this.waitclick(this.findbyxpath(PageObject.WellConfigPage.applybutton),"Apply Button");
		this.waitclick(this.findbyxpath(PageObject.WellConfigPage.commissioninput),"Commission Date");
		this.sendspecialkey(this.findbyxpath(PageObject.WellConfigPage.commissioninput), "selectall");
		this.sendspecialkey(this.findbyxpath(PageObject.WellConfigPage.commissioninput), "delete");
		this.findbyxpath(PageObject.WellConfigPage.commissioninput).sendKeys("01022018");
		this.waitclick(this.findbyid(PageObject.WellConfigPage.modelImport),"Model Import Button");		
		browser.setFileDetector(new remote.FileDetector());
		var fileToUpload = '../TestData/Esp_ProductionTestData.wflx';
		var absolutePath = path.resolve(__dirname, fileToUpload);
		var fileElem = element(by.css('input[type="file"]'));
		browser.executeScript("arguments[0].style.visibility = 'visible'; arguments[0].style.height = '1px'; arguments[0].style.width = '1px';  arguments[0].style.opacity = 1", fileElem.getWebElement());
		fileElem.sendKeys(absolutePath);
		browser.driver.sleep(1000);		//taking a breathe not related to synchronization
		this.waitclick(this.findbyxpath(PageObject.WellConfigPage.applicabledate),"Applicable Date Input");
		this.sendspecialkey(this.findbyxpath(PageObject.WellConfigPage.applicabledate), "selectall");
		this.sendspecialkey(this.findbyxpath(PageObject.WellConfigPage.applicabledate), "delete");
		this.findbyxpath(PageObject.WellConfigPage.applicabledate).sendKeys("12202018");
		this.findbyxpath(PageObject.WellConfigPage.comment).sendKeys("test");
		this.findbyxpath(PageObject.WellConfigPage.tunningmethodlfactor);	
		this.waitclick(this.findbyxpath(PageObject.WellConfigPage.tunningmethoddrpdwn),"Tunnig Method dropdown");		
		this.waitclick(this.findbyxpath(PageObject.WellConfigPage.tunningmethodlfactor),"Tunnig Method name list");		
		this.waitclick(this.findbyxpath(PageObject.WellConfigPage.applybutton),"Apply Button");		
		this.findbyxpath(PageObject.WellConfigPage.wellboreinput).sendKeys(this.getRandomNum(1000,5000));
		this.findbyxpath(PageObject.WellConfigPage.boreholeinput).sendKeys(this.getRandomString(8));
		this.findbyxpath(PageObject.WellConfigPage.intervalinput).sendKeys(this.getRandomString(8));		
		this.waitclick(this.findbyxpath(PageObject.WellConfigPage.savebutton),"Save Button");		
		this.waitforload();
		this.toastverify("Well "+name+" saved successfully", "Well could not be saved");
		break;
		case "rrl":
		this.waitforload();
		var wconf=this.findbyxpath(PageObject.WellConfigPage.wellconfigurationTab);
		var until = protractor.ExpectedConditions;
		browser.wait(until.elementToBeClickable(wconf), 2000)
		.then((found) => {
			wconf.click();
		})
		.catch((waitError) => {	 
			this.waitclick(this.findbyxpath(PageObject.WellConfigPage.configurationTab), "ConfigurationTab");
			this.waitforload();
			this.waitclick(this.findbyxpath(PageObject.WellConfigPage.wellconfigurationTab), "WellConfigTab");
		});							
		this.waitforload();
		var elem=this.findbyxpath(PageObject.WellConfigPage.createnew);
		var until = protractor.ExpectedConditions;
		browser.wait(until.elementToBeClickable(elem), 2000)
		.then((found) => {
			elem.click();
		})
		.catch((waitError) => {	 
		  
		});
		this.waitclick(this.findbyxpath(PageObject.WellConfigPage.general), "General Tab");		
		name="RRL"+this.getRandomString(4);
		this.findbyid(PageObject.WellConfigPage.wellnameinput).sendKeys(name);
		this.waitclick(this.findbyid(PageObject.WellConfigPage.welltypedropdwn),"WellType Dropdown");
		this.waitclick(this.findbyxpath(PageObject.WellConfigPage.welltypeRRL), "WlTyp Name List");
		this.waitclick(this.findbyid(PageObject.WellConfigPage.scadatypedrpdwn),"SCada Dropdown"); 
		this.waitclick(this.findbyxpath(PageObject.WellConfigPage.scadatypename), "Scada Type Name"); 
		this.waitclick(this.findbyid(PageObject.WellConfigPage.cygnetdomaindrpdwn), "Cygnet Dropdown"); 
		this.waitclick(this.findbyxpath(env.cygnetdomainname),"Domain Name"); 
		this.waitforload();
		this.waitclick(this.findbyxpath(PageObject.WellConfigPage.cygnetservicedrpdwn),"Service Dropdown"); 
		this.waitclick(this.findbyxpath(env.cygnetservicename),"Service Name");
		this.waitforload();
		this.waitclick(this.findbyxpath(PageObject.WellConfigPage.facilitybutton),"Facility button");
		this.waitforload();
		this.waitclick(this.findbyxpath(PageObject.WellConfigPage.firstrowfacility),"First Facility in the list");
		this.waitclick(this.findbyxpath(PageObject.WellConfigPage.applybutton),"Apply Button");
		this.waitclick(this.findbyxpath(PageObject.WellConfigPage.commissioninput),"Commission Date");
		this.sendspecialkey(this.findbyxpath(PageObject.WellConfigPage.commissioninput), "selectall");
		this.sendspecialkey(this.findbyxpath(PageObject.WellConfigPage.commissioninput), "delete");
		this.findbyxpath(PageObject.WellConfigPage.commissioninput).sendKeys("01022018");				
		this.findbyxpath(PageObject.WellConfigPage.wellboreinput).sendKeys(this.getRandomNum(1000,5000));
		this.findbyxpath(PageObject.WellConfigPage.boreholeinput).sendKeys(this.getRandomString(8));
		this.findbyxpath(PageObject.WellConfigPage.intervalinput).sendKeys(this.getRandomString(8));
		this.waitclick(this.findbyxpath(PageObject.WellConfigPage.wellattributetab),"WellAttr Tab");
		this.findbyxpath(PageObject.WellConfigPage.leasename).sendKeys(datas.wellattributes.lease);
		this.findbyxpath(PageObject.WellConfigPage.fieldname).sendKeys(datas.wellattributes.field);
		this.findbyxpath(PageObject.WellConfigPage.engineername).sendKeys(datas.wellattributes.engineer);
		this.findbyxpath(PageObject.WellConfigPage.regionname).sendKeys(datas.wellattributes.region);
		this.findbyxpath(PageObject.WellConfigPage.foremanname).sendKeys(datas.wellattributes.foreman);
		this.findbyxpath(PageObject.WellConfigPage.gaugername).sendKeys(datas.wellattributes.gauger);
		this.waitclick(this.findbyxpath(PageObject.WellConfigPage.savebutton),"Save Button");		
		this.waitforload();
		this.toastverify("Well "+name+" saved successfully", "Well could not be saved");
		
		break;
		default:
		expect("Well should be supported").toEqual("Well type not supported");				
		
		
	}
	return name;//in future will return jsobjects, returning value of facility, borehole, interval and others.
};
this.welltest=function(welltype, unit){
	switch(welltype.toLowerCase()){
		case 'esp':
		var datapath;
		var unitstr=unit.toString();
		unitstr=unitstr.toLowerCase().trim();
		console.log("Unit is:"+unitstr);		
		//if(unitstr!=="metric" || unitstr!=="us"){console.log("Invalid unit , not handled"); expect(true).toEqual(false);}
	
		var wtest=this.findbyxpath(PageObject.OptimizationPage.WellTest);
		var until = protractor.ExpectedConditions;
		browser.wait(until.elementToBeClickable(wtest), 2000)
		.then((found) => {
			wtest.click();
		})
		.catch((waitError) => {	 
			this.waitclick(this.findbyxpath(PageObject.OptimizationPage.optimizationTab), "optimizationTab");
			this.waitforload();
			this.waitclick(this.findbyxpath(PageObject.OptimizationPage.WellTest), "WellTest");
		});			
		this.waitforload();
		this.waitclick(this.findbyxpath(PageObject.OptimizationPage.createwelltest), "createwelltest");
		this.waitforload();
		this.findbyxpath(PageObject.OptimizationPage.chokesize)
		this.waitclick(this.findbyxpath(PageObject.OptimizationPage.testdateinput), "TestDate Inputfield");
		this.sendspecialkey(this.findbyxpath(PageObject.OptimizationPage.testdateinput), "selectall");
		this.sendspecialkey(this.findbyxpath(PageObject.OptimizationPage.testdateinput), "delete");		
		this.findbyxpath(PageObject.OptimizationPage.testdateinput).sendKeys(datas.espwelltestdata.testdate);		
		this.waitclick(this.findbyxpath(PageObject.OptimizationPage.qualitycodedrpdwn), "Quality COde Dropdown");


		if(unitstr=="us"){
		this.waitclick(this.findbyxpath(datas.espwelltestdata.qualitycode), "Quality COde list name");
		this.sendspecialkey(this.findbyxpath(PageObject.OptimizationPage.oilrate), "delete");
		this.findbyxpath(PageObject.OptimizationPage.oilrate).sendKeys(datas.espwelltestdata.oilrate);
		this.sendspecialkey(this.findbyxpath(PageObject.OptimizationPage.waterrate), "delete");		
		this.findbyxpath(PageObject.OptimizationPage.waterrate).sendKeys(datas.espwelltestdata.waterrate);		
		this.sendspecialkey(this.findbyxpath(PageObject.OptimizationPage.gasrate), "delete");		
		this.findbyxpath(PageObject.OptimizationPage.gasrate).sendKeys(datas.espwelltestdata.gasrate);
		this.sendspecialkey(this.findbyxpath(PageObject.OptimizationPage.thp), "delete");		
		this.findbyxpath(PageObject.OptimizationPage.thp).sendKeys(datas.espwelltestdata.thp);
		this.sendspecialkey(this.findbyxpath(PageObject.OptimizationPage.tht), "delete");		
		this.findbyxpath(PageObject.OptimizationPage.tht).sendKeys(datas.espwelltestdata.tht);
		this.sendspecialkey(this.findbyxpath(PageObject.OptimizationPage.chp), "delete");		
		this.findbyxpath(PageObject.OptimizationPage.chp).sendKeys(datas.espwelltestdata.chp);
		//this.findbyxpath(PageObject.OptimizationPage.pip).sendKeys(datapath+pip);
		//this.findbyxpath(PageObject.OptimizationPage.pdp).sendKeys(datas.espwelltestdata.pdp);
		this.sendspecialkey(this.findbyxpath(PageObject.OptimizationPage.freq), "delete");		
		this.findbyxpath(PageObject.OptimizationPage.freq).sendKeys(datas.espwelltestdata.freq);
		this.sendspecialkey(this.findbyxpath(PageObject.OptimizationPage.flp), "delete");		
		this.findbyxpath(PageObject.OptimizationPage.flp).sendKeys(datas.espwelltestdata.flp);
		this.sendspecialkey(this.findbyxpath(PageObject.OptimizationPage.sp), "delete");		
		this.findbyxpath(PageObject.OptimizationPage.sp).sendKeys(datas.espwelltestdata.sp);
		this.sendspecialkey(this.findbyxpath(PageObject.OptimizationPage.chokesize), "delete");		
		this.findbyxpath(PageObject.OptimizationPage.chokesize).sendKeys(datas.espwelltestdata.chokesize);
	}

	if(unitstr=="metric"){
		this.waitclick(this.findbyxpath(datas.espwelltestdatametric.qualitycode), "Quality COde list name");
		this.sendspecialkey(this.findbyxpath(PageObject.OptimizationPage.oilrate), "delete");
		this.findbyxpath(PageObject.OptimizationPage.oilrate).sendKeys(datas.espwelltestdatametric.oilrate);
		this.sendspecialkey(this.findbyxpath(PageObject.OptimizationPage.waterrate), "delete");		
		this.findbyxpath(PageObject.OptimizationPage.waterrate).sendKeys(datas.espwelltestdatametric.waterrate);		
		this.sendspecialkey(this.findbyxpath(PageObject.OptimizationPage.gasrate), "delete");		
		this.findbyxpath(PageObject.OptimizationPage.gasrate).sendKeys(datas.espwelltestdatametric.gasrate);
		this.sendspecialkey(this.findbyxpath(PageObject.OptimizationPage.thp), "delete");		
		this.findbyxpath(PageObject.OptimizationPage.thp).sendKeys(datas.espwelltestdatametric.thp);
		this.sendspecialkey(this.findbyxpath(PageObject.OptimizationPage.tht), "delete");		
		this.findbyxpath(PageObject.OptimizationPage.tht).sendKeys(datas.espwelltestdatametric.tht);
		this.sendspecialkey(this.findbyxpath(PageObject.OptimizationPage.chp), "delete");		
		this.findbyxpath(PageObject.OptimizationPage.chp).sendKeys(datas.espwelltestdatametric.chp);
		//this.findbyxpath(PageObject.OptimizationPage.pip).sendKeys(datapath+pip);
		//this.findbyxpath(PageObject.OptimizationPage.pdp).sendKeys(datas.espwelltestdata.pdp);
		this.sendspecialkey(this.findbyxpath(PageObject.OptimizationPage.freq), "delete");		
		this.findbyxpath(PageObject.OptimizationPage.freq).sendKeys(datas.espwelltestdatametric.freq);
		this.sendspecialkey(this.findbyxpath(PageObject.OptimizationPage.flp), "delete");		
		this.findbyxpath(PageObject.OptimizationPage.flp).sendKeys(datas.espwelltestdatametric.flp);
		this.sendspecialkey(this.findbyxpath(PageObject.OptimizationPage.sp), "delete");		
		this.findbyxpath(PageObject.OptimizationPage.sp).sendKeys(datas.espwelltestdatametric.sp);
		this.sendspecialkey(this.findbyxpath(PageObject.OptimizationPage.chokesize), "delete");		
		this.findbyxpath(PageObject.OptimizationPage.chokesize).sendKeys(datas.espwelltestdatametric.chokesize);	
	
	}





		this.waitclick(this.findbyxpath(PageObject.OptimizationPage.save),"Save Button");	
		this.waitforload();		
		this.toastverify("Saved successfully", "Well Test could not be saved");
		expect("Success").toContain(this.getspantext(this.findbyxpath(PageObject.OptimizationPage.statuscol), "Text couldnot be found"), "Status is not Success");	
		break;
		default:
		expect("Well should be supported").toEqual("Well type not supported");		
	}
};
this.getRandomString = function(length) {
	var string = '';
	var letters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz' //Include numbers if you want
			for (i = 0; i < length; i++) {
				string += letters.charAt(Math.floor(Math.random() * letters.length));
			}
			return string;
		};
this.getRandomNum = function(min, max){
			return parseInt(Math.random() * (max - min) + min);
		};
};
module.exports=new utility();

