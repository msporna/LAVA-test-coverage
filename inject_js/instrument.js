/*
 * Copyright (c) 2016 by Michal Sporna and contributors.  See AUTHORS
for more details.

Some rights reserved.

Redistribution and use in source and binary forms of the software as well
as documentation, with or without modification, are permitted provided
that the following conditions are met:

* Redistributions of source code must retain the above copyright
  notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above
  copyright notice, this list of conditions and the following
  disclaimer in the documentation and/or other materials provided
  with the distribution.

* The names of the contributors may not be used to endorse or
  promote products derived from this software without specific
  prior written permission.

THIS SOFTWARE AND DOCUMENTATION IS PROVIDED BY THE COPYRIGHT HOLDERS AND
CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT
NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER
OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE AND DOCUMENTATION, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
DAMAGE.
 */

//does not require jquery
//usage:var instrumenter=new jsInstrument(params here);
//method call example: instrumenter.InstrumentCode();
var jsInstrument = function(serverURL_p) {
  //save values for later use
  this.serverURL = serverURL_p;

  this.executed_count = 0;
  this.executed_lines = [];

  /*
     * function that sends instrumentation stats to backend
     */
  this.InstrumentCode = function(
    line_guid,
    file_p,
    related_code_line,
    inject_type
  ) {
    this.executed_lines.push({
      file: file_p,
      line_guid: line_guid,
      related_code_line: related_code_line,
      inject_type: inject_type
    });

    //send to backend
    var request = new XMLHttpRequest();
    request.open(
      "GET",
      this.serverURL +
        "/send_instrumentation_stats" +
        "?file=" +
        file_p +
        "&line_guid_p=" +
        line_guid +
        "&route=" +
        window.location.href +
        "&related_code_line=" +
        related_code_line +
        "&inject_type=" +
        inject_type,
      true
    );
    request.send();
  };
};
