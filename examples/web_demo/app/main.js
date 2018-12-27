var FUNCTION_NAME = "function3";
var VALUE_1 = 100;

//called by button 1
function function1() {
  console.log("f1");
  function3();
}

//called by button 2
function function2() {
  set_output("function 2 output");


  switch (VALUE_1) {
    case 100:
      console.log("100!");
      VALUE_1 = 200;
      break;
    case 200:
      console.log("200!");
      break;
    default:
      console.log("error!");
  }
}

//called by function1
function function3() {
  set_output("function 3 output");



  if (FUNCTION_NAME == "function3") {
    console.log("function3 if");
    FUNCTION_NAME = "function2";
  } else if (FUNCTION_NAME == "function2" || FUNCTION_NAME == "function1") {
    console.log("function 3??");
    FUNCTION_NAME = "other";
  } else {
    console.log("other function");
  }
}

//called by function2 and function3
function set_output(output) {
  $("#output_paragraph").text(output);
  for (var i = 0; i < 5; i++) {
    console.log("out");
  }
}