#library
add_executable(zitistructalignment2 zitistructalignment.c)

set_property(TARGET zitistructalignment2 PROPERTY C_STANDARD 11)

target_link_libraries(zitistructalignment2
        PUBLIC ziti
        )