import sqlite3

PATH = 'instrument.db'


def create_db():
    conn = sqlite3.connect(PATH)
    c = conn.cursor()

    ##########################################################################
    ##################  T A B L E       C R E A T I O N S   ##################
    ##################                                      ##################
    ##########################################################################

    '''
    [SESSIONS] table
    '''
    c.execute('''CREATE TABLE sessions(ID INTEGER PRIMARY KEY AUTOINCREMENT,is_over INTEGER,name VARCHAR(160),total_coverage VARCHAR(10),start_time DATETIME,end_time DATETIME,current_active_modules_count INTEGER)''')

    '''
    [STATS] table
    '''
    c.execute('''CREATE TABLE stats(ID INTEGER PRIMARY KEY AUTOINCREMENT,file_id INTEGER, session_id INTEGER, date DATETIME, filename VARCHAR(4000), line INTEGER, line_guid VARCHAR(1000), coverage_type VARCHAR(200))''')

    c.execute('''CREATE TABLE config(ID INTEGER PRIMARY KEY AUTOINCREMENT,name VARCHAR(100), value VARCHAR(200))''')

    '''
    [FILES] table
    When upload package contains file that does not exist in db, insert file along with content. If upload package contains file
    that exists in db with history=true, make it history=false. If db contains file that is not in upload package mark that file as history.

    ID
    name
    path
    type
    should_instrument
    is_history - if there is an upload of files from instrumenter and some file exists in db but not in upload package, mark the file as history=true.
    '''
    c.execute('''CREATE TABLE files(ID INTEGER PRIMARY KEY AUTOINCREMENT,name VARCHAR(100), path VARCHAR(4000), type VARCHAR(20), should_instrument INTEGER, is_history INTEGER)''')

    '''
    [VISITED_ROUTES] table

    '''
    c.execute('''CREATE TABLE visited_routes(ID INTEGER PRIMARY KEY AUTOINCREMENT,route_visited VARCHAR(100),session_id INTEGER)''')

    '''
    [FILE_DETAILS] table
    ID
    file_id
    file_content
    executable_lines_count

    '''
    c.execute(
        '''CREATE TABLE file_details(ID INTEGER PRIMARY KEY AUTOINCREMENT,file_id INTEGER, file_content BLOB, executable_lines_count INTEGER,updated DATETIME,executable_lines VARCHAR(4000))''')

    '''
    [ROUTES] table
    ID
    route
    is_history - if upload package hasn't got route that exist in db, mark that as history=1 for reference
    '''
    c.execute(
        '''CREATE TABLE routes(ID INTEGER PRIMARY KEY AUTOINCREMENT,route VARCHAR(100),is_history INTEGER)''')

    '''
    [MODULES] table

    ID
    module_name - unique module name given by user in Modules section of the dashboard
    related_files - comma separated file id's
    last_update - date when last update was performed which resulted in revision being increased
    is_removed - true if user completely removed it (0/1); can set history=1 as well.
    operation - files_update,rename,delete
    is_history -when user updates module, eg: renames or file assignment then current module's entry becomes history; new entry is inserted for updated state.
    '''
    c.execute(
        '''CREATE TABLE modules(ID INTEGER PRIMARY KEY AUTOINCREMENT,module_name VARCHAR(100),related_files VARCHAR(4000),last_update DATETIME,is_removed INTEGER,operation VARCHAR(100),is_history INTEGER)''')

    '''
    [COVERED_MODULES] table
    when session starts, list of modules is specified - can select all or 1 at minimum
    this table reflects that


    ID
    module_id - module id that is related to session; this is ID of module entry from modules table.
    session_id
    total_coverage - total coverage for the whole module (all files in it combined) expressed in percentage

    '''
    c.execute('''CREATE TABLE covered_modules(ID INTEGER PRIMARY KEY AUTOINCREMENT,module_id INTEGER,session_id INTEGER,total_coverage VARCHAR(10))''')

    '''
    [SESSIONS_FILES] table
    files included in session are the ones belonging to modules chosen when creating session
    example: chosen 1 module out of 5, so only files from module 1 are in session.


    '''
    c.execute(
        '''CREATE TABLE sessions_files(session_id INTEGER, file_id INTEGER,file_details INTEGER)''')

    ##########################################################################
    ##################  C O N F I G     E N T R I E S   ######################
    ##################                                  ######################
    ##########################################################################

    c.execute("INSERT INTO config(name,value) VALUES(?,?)",
              ("SESSION_TIMEOUT", 3600))
    c.execute("INSERT INTO config(name,value) VALUES(?,?)", ("PORT", 5000))
    c.execute("INSERT INTO config(name,value) VALUES(?,?)",
              ("UPDATE_INTERVAL_SECONDS", 10))
    c.execute("INSERT INTO config(name,value) VALUES(?,?)",
              ("SERVER_HOST", "0.0.0.0"))
    c.execute("INSERT INTO config(name,value) VALUES(?,?)",
              ("VERSION", "2.0.0"))
    c.execute("INSERT INTO config(name,value) VALUES(?,?)",
              ("PROJECT_NAME", "Sample Project"))

    conn.commit()
    conn.close()


if __name__ == '__main__':
    create_db()
